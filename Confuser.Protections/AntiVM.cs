﻿using System;
using System.Collections.Generic;
using System.Linq;
using Confuser.Core;
using Confuser.Core.Helpers;
using Confuser.Core.Services;
using Confuser.Renamer;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace Confuser.Protections
{
	[BeforeProtection(new string[]
	{
		"Ki.ControlFlow",
		"Ki.Constants"
	})]
	internal class AntiVMProtection : Protection
	{
		public override string Name
		{
			get
			{
				return "Anti VM Protection";
			}
		}
		public override string Description
		{
			get
			{
				return "This protection will prevent your program from running in a Virtual Machine";
			}
		}
		public override string Id
		{
			get
			{
				return "anti vm";
			}
		}
		public override string FullId
		{
			get
			{
				return "Ki.AntiVM";
			}
		}

		public override ProtectionPreset Preset
		{
			get
			{
				return ProtectionPreset.Maximum;
			}
		}

		protected override void Initialize(ConfuserContext context)
		{
		}

		protected override void PopulatePipeline(ProtectionPipeline pipeline)
		{
			pipeline.InsertPreStage(PipelineStage.ProcessModule, new AntiVMProtection.AntiVMPhase(this));
		}

		public const string _Id = "anti vm";

		public const string _FullId = "Ki.AntiVM";

		private class AntiVMPhase : ProtectionPhase
		{
			public AntiVMPhase(AntiVMProtection parent) : base(parent)
			{
			}

			public override ProtectionTargets Targets
			{
				get
				{
					return ProtectionTargets.Modules;
				}
			}

			public override string Name
			{
				get
				{
					return "Anti VM";
				}
			}

			protected override void Execute(ConfuserContext context, ProtectionParameters parameters)
			{
				TypeDef runtimeType = context.Registry.GetService<IRuntimeService>().GetRuntimeType("Confuser.Runtime.AntiVM");
				IMarkerService service = context.Registry.GetService<IMarkerService>();
				INameService service2 = context.Registry.GetService<INameService>();
				foreach (ModuleDef moduleDef in parameters.Targets.OfType<ModuleDef>())
				{
					IEnumerable<IDnlibDef> enumerable = InjectHelper.Inject(runtimeType, moduleDef.GlobalType, moduleDef);
					MethodDef methodDef = moduleDef.GlobalType.FindStaticConstructor();
					MethodDef method2 = (MethodDef)enumerable.Single((IDnlibDef method) => method.Name == "Init");
					methodDef.Body.Instructions.Insert(0, Instruction.Create(OpCodes.Call, method2));
					foreach (IDnlibDef def in enumerable)
					{
						service2.MarkHelper(def, service, (Protection)base.Parent);
					}
				}
			}
		}
	}
}
