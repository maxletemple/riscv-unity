using System;
using UnityEngine;

public class Cpu
{
	private Reg64 pc;
	private Reg64[] registers = new Reg64[32];
	private MemoryBus memoryBus;

	public Cpu(MemoryBus memoryBus)
	{
		this.memoryBus = memoryBus;
		pc = 0;
		for (int i = 0; i < registers.Length; i++)
		{
			registers[i] = 0;
		}
	}

	public void DoCycle()
	{
		// Debug.Log($"PC: {pc}, Registers: [{string.Join(", ", Array.ConvertAll(registers, r => r.ToString()))}]");
		Instruction instruction = new Instruction(memoryBus.Read32(pc));
		Reg64 instr_imm = instruction.imm;
		switch (instruction.opcode)
		{
			case opcodes.LUI:
				registers[instruction.rd] = (UInt64)(instruction.imm << 12);
				break;
			case opcodes.AUIPC:
				registers[instruction.rd] = (UInt64)((Int64)pc + instr_imm.getSigned(20) << 12);
				break;
			case opcodes.JAL:
				registers[instruction.rd] = pc + 4;
				pc = pc + instr_imm.getSigned(20);
				return;
			case opcodes.JALR:
				registers[instruction.rd] = pc + 4;
				pc = pc + instr_imm.getSigned(12) + registers[instruction.rs1];
				pc = pc >> 1 << 1; // Ensure the address is even
				return;
			case opcodes.BRANCH:
				switch (instruction.funct3)
				{
					case funct3.BEQ:
						if (registers[instruction.rs1] == registers[instruction.rs2])
						{
							pc += instr_imm.getSigned(13);
						}
						break;
					case funct3.BNE:
						if (registers[instruction.rs1] != registers[instruction.rs2])
						{
							pc += instr_imm.getSigned(13);
						}
						break;
					case funct3.BLT:
						if (registers[instruction.rs1].getSigned(13) < registers[instruction.rs2].getSigned(13))
						{
							pc += instr_imm.getSigned(13);
						}
						break;
					case funct3.BGE:
						if (registers[instruction.rs1].getSigned(13) > registers[instruction.rs2].getSigned(13))
						{
							pc += instr_imm.getSigned(13);
						}
						break;
					case funct3.BLTU:
						if (registers[instruction.rs1] < registers[instruction.rs2])
						{
							pc += instr_imm.getSigned(13);
						}
						break;
					case funct3.BGEU:
						if (registers[instruction.rs1] >= registers[instruction.rs2])
						{
							pc += instr_imm.getSigned(13);
						}
						break;
				}
				return;
			case opcodes.LOAD:
				switch (instruction.funct3)
				{
					case funct3.LB:
						registers[instruction.rd] = memoryBus.Read8(registers[instruction.rs1] + instr_imm.getSigned(12));
						registers[instruction.rd] = registers[instruction.rd].getSigned(8);
						break;
					case funct3.LBU:
						registers[instruction.rd] = memoryBus.Read8(registers[instruction.rs1] + instr_imm.getSigned(12));
						break;
					case funct3.LH:
						registers[instruction.rd] = memoryBus.Read16(registers[instruction.rs1] + instr_imm.getSigned(12));
						registers[instruction.rd] = registers[instruction.rd].getSigned(16);
						break;
					case funct3.LHU:
						registers[instruction.rd] = memoryBus.Read16(registers[instruction.rs1] + instr_imm.getSigned(12));
						break;
					case funct3.LW:
						registers[instruction.rd] = memoryBus.Read32(registers[instruction.rs1] + instr_imm.getSigned(12));
						break;
					case funct3.LWU:
						registers[instruction.rd] = memoryBus.Read32(registers[instruction.rs1] + instr_imm.getSigned(12));
						registers[instruction.rd] = registers[instruction.rd].getUnsigned(32);
						break;
					case funct3.LD:
						registers[instruction.rd] = memoryBus.Read64(registers[instruction.rs1] + instr_imm.getSigned(12));
						registers[instruction.rd] = registers[instruction.rd].getSigned(64);
						break;
					default:
						throw new InvalidOperationException("Unknown LOAD funct3: " + instruction.funct3);
				}
				break;
			case opcodes.STORE:
				switch (instruction.funct3)
				{
					case funct3.SB:
						memoryBus.Write8(registers[instruction.rs1] + instr_imm.getSigned(12), registers[instruction.rs2]);
						break;
					case funct3.SH:
						memoryBus.Write16(registers[instruction.rs1] + instr_imm.getSigned(12), registers[instruction.rs2]);
						break;
					case funct3.SW:
						memoryBus.Write32(registers[instruction.rs1] + instr_imm.getSigned(12), registers[instruction.rs2]);
						break;
					case funct3.SD:
						memoryBus.Write64(registers[instruction.rs1] + instr_imm.getSigned(12), registers[instruction.rs2]);
						break;
					default:
						throw new InvalidOperationException("Unknown STORE funct3: " + instruction.funct3);
				}
				break;
			case opcodes.ALU_IMM:
				switch (instruction.funct3)
				{
					case funct3.ADDI:
						registers[instruction.rd] = registers[instruction.rs1] + instr_imm.getSigned(12);
						break;
					case funct3.SLTI:
						registers[instruction.rd] = registers[instruction.rs1].getSigned(64) < instr_imm.getSigned(12) ? 1 : 0;
						break;
					case funct3.SLTIU:
						registers[instruction.rd] = registers[instruction.rs1] < instr_imm.getUnsigned(12) ? 1 : 0;
						break;
					case funct3.XORI:
						registers[instruction.rd] = registers[instruction.rs1] ^ instr_imm.getUnsigned(12);
						break;
					case funct3.ORI:
						registers[instruction.rd] = registers[instruction.rs1] | instr_imm.getSigned(12);
						break;
					case funct3.ANDI:
						registers[instruction.rd] = registers[instruction.rs1] & instr_imm.getSigned(12);
						break;
					case funct3.SLLI:
						registers[instruction.rd] = registers[instruction.rs1] << instruction.shamt;
						break;
					case funct3.SRLI: // Also SRAI
						if (instruction.funct7 == funct7.SRLI)
						{
							registers[instruction.rd] = registers[instruction.rs1] >> instruction.shamt;
						}
						else if (instruction.funct7 == funct7.SRAI)
						{
							registers[instruction.rd] = registers[instruction.rs1].getSigned(64) >> instruction.shamt;
						}
						else
						{
							throw new InvalidOperationException("Unknown SRLI/SRAI funct7: " + instruction.funct7);
						}
						break;
				}
				break;
			case opcodes.ALU_IMM_64:
				switch (instruction.funct3)
				{
					case funct3.SLLIW:
						registers[instruction.rd] = registers[instruction.rs1].getSigned(32) << instruction.shamt;
						break;
					case funct3.SRLIW:
						if (instruction.funct7 == funct7.SRLI)
						{
							registers[instruction.rd] = registers[instruction.rs1].getSigned(32) >> instruction.shamt;
						}
						else if (instruction.funct7 == funct7.SRAI)
						{
							registers[instruction.rd] = registers[instruction.rs1].getSigned(32) >> instruction.shamt;
						}
						else
						{
							throw new InvalidOperationException("Unknown SRLIW/SRAIW funct7: " + instruction.funct7);
						}
						break;

				}
				break;
			case opcodes.ALU:
				switch (instruction.funct3)
				{
					case funct3.ADD:
						if (instruction.funct7 == funct7.ADD)
						{
							registers[instruction.rd] = registers[instruction.rs1].getSigned(64) + registers[instruction.rs2].getSigned(64);
						}
						else if (instruction.funct7 == funct7.SUB)
						{
							registers[instruction.rd] = registers[instruction.rs1].getSigned(64) - registers[instruction.rs2].getSigned(64);
						}
						else
						{
							throw new InvalidOperationException("Unknown ADD/SUB funct7: " + instruction.funct7);
						}
						break;
					case funct3.SLL:
						registers[instruction.rd] = registers[instruction.rs1] << (int)registers[instruction.rs2].getUnsigned(64);
						break;
					case funct3.SLT:
						registers[instruction.rd] = registers[instruction.rs1].getSigned(64) < registers[instruction.rs2].getSigned(64) ? 1 : 0;
						break;
					case funct3.SLTU:
						registers[instruction.rd] = registers[instruction.rs1] < registers[instruction.rs2] ? 1 : 0;
						break;
					case funct3.XOR:
						registers[instruction.rd] = registers[instruction.rs1] ^ registers[instruction.rs2];
						break;
					case funct3.SRL:
						if (instruction.funct7 == funct7.SRL)
						{
							registers[instruction.rd] = registers[instruction.rs1] >> (int)registers[instruction.rs2].getUnsigned(64);
						}
						else if (instruction.funct7 == funct7.SRA)
						{
							registers[instruction.rd] = registers[instruction.rs1] >> (int)registers[instruction.rs2].getUnsigned(64);
						}
						else
						{
							throw new InvalidOperationException("Unknown SRL/SRA funct7: " + instruction.funct7);
						}
						break;
					case funct3.OR:
						registers[instruction.rd] = registers[instruction.rs1] | registers[instruction.rs2];
						break;
					case funct3.AND:
						registers[instruction.rd] = registers[instruction.rs1] & registers[instruction.rs2];
						break;
					default:
						throw new InvalidOperationException("Unknown ALU funct3: " + instruction.funct3);
				}
				break;
			default:
				throw new InvalidOperationException("Unknown opcode: " + instruction.opcode);
		}
		pc += 4;
	}

	public override string ToString()
	{
		string regValues = string.Join(", ", Array.ConvertAll(registers, r => r.ToString()));
		return $"PC: {pc}, Registers: [{regValues}]";
	}
}