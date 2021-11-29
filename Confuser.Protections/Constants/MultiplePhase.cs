﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Confuser.Core;
using Confuser.Core.Helpers;
using Confuser.Core.Services;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace Confuser.Protections.Constants {
	internal class MultiplePhase : ProtectionPhase {
		public MultiplePhase(ConstantProtection parent)
			: base(parent) { }

		public override ProtectionTargets Targets {
			get { return ProtectionTargets.Modules; }
		}

		public override string Name {
			get { return "Constants Mutation"; }
		}

		protected override void Execute(ConfuserContext context, ProtectionParameters parameters) {
			
            foreach (ModuleDef module in parameters.Targets.OfType<ModuleDef>())
            {
                foreach (TypeDef type in module.Types)
                {
                    if (type.IsGlobalModuleType) continue;
                    foreach (MethodDef method in type.Methods)
                    {
                        if (method.FullName.Contains("My.")) continue;

                        if (method.IsConstructor) continue;
                        if (!method.HasBody) continue;
                        var instr = method.Body.Instructions;
                        for (int i = 0; i < method.Body.Instructions.Count; i++)
                        {
                            if (instr[i].OpCode == OpCodes.Ldstr)
                            {
                                Random rn = new Random();
                                for (int j = 1; j < rn.Next(16); j++)
                                {
                                    if (j != 1) j += 1;
                                    //Create a new local 
                                    Local new_local = new Local(module.CorLibTypes.String);
                                    //Create another new local
                                    Local new_local2 = new Local(module.CorLibTypes.String);
                                    //add them in the method
                                    method.Body.Variables.Add(new_local);
                                    method.Body.Variables.Add(new_local2);
                                    //set ldstr value to the local
                                    instr.Insert(i + j, Instruction.Create(OpCodes.Stloc_S, new_local));
                                    instr.Insert(i + (j + 1), Instruction.Create(OpCodes.Ldloc_S, new_local));
                                }
                            }
                            if (instr[i].IsLdcI4())
                            {
                                Random rn = new Random();
                                for (int j = 1; j < rn.Next(16); j++)
                                {
                                    if (j != 1) j += 1;
                                    Local new_local = new Local(module.CorLibTypes.Int32);
                                    Local new_local2 = new Local(module.CorLibTypes.Int32);
                                    method.Body.Variables.Add(new_local);
                                    method.Body.Variables.Add(new_local2);
                                    instr.Insert(i + j, Instruction.Create(OpCodes.Stloc_S, new_local));
                                    instr.Insert(i + (j + 1), Instruction.Create(OpCodes.Ldloc_S, new_local));
                                }
                            }
                        }
                    }
                }
            } // Mutations
        }

		
	}
}