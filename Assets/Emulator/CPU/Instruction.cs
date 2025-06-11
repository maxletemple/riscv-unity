using System;
using System.Text;
using UnityEngine;

public enum InstructionType
{
    R,
    I,
    S,
    B,
    U,
    J
}

public enum opcodes
{
    LUI = 0b0110111,
    AUIPC = 0b0010111,
    JAL = 0b1101111,
    JALR = 0b1100111,
    BRANCH = 0b1100011,
    LOAD = 0b0000011,
    STORE = 0b0100011,
    ALU_IMM = 0b0010011,
    ALU_IMM_64 = 0b0011011,
    ALU = 0b0110011,
    FENCE = 0b0001111,
    SYSTEM = 0b1110011,
    ATOMIC = 0b0101111
}

public enum funct3
{
    BEQ = 0b000,
    BNE = 0b001,
    BLT = 0b100,
    BGE = 0b101,
    BLTU = 0b110,
    BGEU = 0b111,

    LB = 0b000,
    LH = 0b001,
    LW = 0b010,
    LWU = 0b110,
    LBU = 0b100,
    LHU = 0b101,
    LD = 0b011,

    SB = 0b000,
    SH = 0b001,
    SW = 0b010,
    SD = 0b011,

    ADDI = 0b000,
    SLTI = 0b010,
    SLTIU = 0b011,
    XORI = 0b100,
    ORI = 0b110,
    ANDI = 0b111,
    SLLI = 0b001,
    SRLI = 0b101,
    SRAI = 0b101,

    ADDIW = 0b000,
    SLLIW = 0b001,
    SRLIW = 0b101,
    SRAIW = 0b101,

    MUL = 0b000,
    MULH = 0b001,
    MULHSU = 0b010,
    MULHU = 0b011,
    DIV = 0b100,
    DIVU = 0b101,
    REM = 0b110,
    REMU = 0b111,

    ADD = 0b000,
    SUB = 0b000,
    SLL = 0b001,
    SLT = 0b010,
    SLTU = 0b011,
    XOR = 0b100,
    SRL = 0b101,
    SRA = 0b101,
    OR = 0b110,
    AND = 0b111,

    ECALL = 0b000,
    EBREAK = 0b000,
    CSRRW = 0b001,
    CSRRS = 0b010,
    CSRRC = 0b011,
    CSRRWI = 0b101,
    CSRRSI = 0b110,
    CSRRCI = 0b111,
}

public enum funct7
{
    ADD = 0b0000000,
    MUL = 0b0000001,
    SUB = 0b0100000,
    SLL = 0b0000000,
    SLT = 0b0000000,
    SLTU = 0b0000000,
    XOR = 0b0000000,
    SRL = 0b0000000,
    SRA = 0b0100000,
    OR = 0b0000000,
    AND = 0b0000000,

    SRLI = 0b0000000,
    SRAI = 0b0100000,
}

public enum aom_op
{
    AMOADD = 0b00000,
    AMOSWAP = 0b00001,
    LR = 0b00010,
    SC = 0b00011,
    AMOXOR = 0b00100,
    AMOAND = 0b01100,
    AMOOR = 0b01000,
    AMOMIN = 0b10000,
    AMOMAX = 0b10100,
    AMOMINU = 0b11000,
    AMOMAXU = 0b11100,
}

public struct Instruction
{
    public opcodes opcode;
    public int rd;
    public int rs1;
    public int rs2;
    public int imm;
    public funct3 funct3;
    public funct7 funct7;
    public int shamt;
    public bool isCompressed;

    public Instruction(UInt32 instruction)
    {
        ushort c_opcode = (ushort)(instruction & 0b11);
        isCompressed = c_opcode != 0b11;
        opcode = 0;
        rs1 = 0;
        rs2 = 0;
        rd = 0;
        imm = 0;
        shamt = 0;
        funct3 = 0;
        funct7 = 0;

        if (isCompressed)
        {
            instruction &= 0xffff;
            var c_funct3 = instruction >> 13;
            switch (c_opcode)
            {
                case 0b00:
                    switch (c_funct3)
                    {
                        case 0b000: // c.addi4spn
                            if (instruction != 0)
                            {
                                opcode = opcodes.ALU_IMM;
                                funct3 = funct3.ADDI;
                                imm = (int)(((instruction >> 2) & 0b1000) | ((instruction >> 4) & 0b100) | ((instruction >> 1) & 0b1111000000) | ((instruction >> 8) & 0b110000));
                                rs1 = 0x2;
                                rd = 8 + (int)((instruction >> 2) & 0b111);
                            }
                            break;
                        case 0b010: // c.lw
                            opcode = opcodes.LOAD;
                            funct3 = funct3.LW;
                            imm = (int)(((instruction >> 4) & 0b100) | ((instruction >> 7) & 0b111000) | (instruction & 0b1000000));
                            rs1 = 8 + (int)((instruction >> 7) & 0b111);
                            rd = 8 + (int)((instruction >> 2) & 0b111);
                            break;
                        case 0b110: // c.sw
                            opcode = opcodes.STORE;
                            funct3 = funct3.SW;
                            imm = (int)(((instruction >> 4) & 0b100) | ((instruction >> 7) & 0b111000) | (instruction & 0b1000000));
                            rs1 = 8 + (int)((instruction >> 7) & 0b111);
                            rs2 = 8 + (int)((instruction >> 2) & 0b111);
                            break;
                        default:
                            throw new InvalidOperationException("Illegal compressed instruction");
                    }
                    break;
                case 0b01:
                    switch (c_funct3)
                    {
                        case 0b000: // c.addi
                            opcode = opcodes.ALU_IMM;
                            funct3 = funct3.ADDI;
                            imm = (int)(((instruction >> 1) & 0b11111) | ((instruction >> 7) & 0b100000));
                            imm = (int)((Reg64)imm).getSigned(6);
                            rd = (int)((instruction >> 7) & 0b11111);
                            rs1 = rd;
                            break;
                        case 0b001: // c.jal
                            opcode = opcodes.JAL;
                            imm = (int)(((instruction >> 2) & 0b1110) | ((instruction >> 7) & 0b10000) |
                                        ((instruction << 3) & 0b100000) | ((instruction >> 1) & 0b1000000) |
                                        ((instruction << 1) & 0b10000000) | ((instruction >> 1) & 0b1100000000) |
                                        ((instruction << 2) & 0b10000000000) | ((instruction >> 1) & 0b100000000000));
                            imm = (int)((Reg64)imm).getSigned(12);
                            rd = 0x1;
                            break;
                        case 0b010: // c.li
                            opcode = opcodes.ALU_IMM;
                            funct3 = funct3.ADDI;
                            imm = (int)(((instruction >> 2) & 0b11111) | ((instruction >> 7) & 0b100000));
                            imm = (int)((Reg64)imm).getSigned(6);
                            rd = (int)((instruction >> 7) & 0b11111);
                            rs1 = 0x0;
                            break;
                        case 0b011:
                            rd = (int)((instruction >> 7) & 0b111);
                            if (rd == 0x2) // c.addi16sp
                            {
                                opcode = opcodes.ALU_IMM;
                                funct3 = funct3.ADDI;
                                rs1 = 0x2;
                                rd = 0x2;
                                imm = (int)(((instruction >> 2) & 0b10000) | ((instruction << 3) & 0b100000) |
                                            ((instruction << 1) & 0b1000000) | ((instruction << 4) & 0b110000000) |
                                            ((instruction >> 3) & 0b1000000000));
                                imm = (int)((Reg64)imm).getSigned(10);
                            }
                            else if (rd != 0x0) // c.lui
                            {
                                opcode = opcodes.LUI;
                                imm = (int)(((instruction >> 2) & 0b11) | ((instruction >> 10) & 0b100));
                                rd = (int)((instruction >> 7) & 0b11111);
                                imm = (int)((Reg64)imm).getSigned(18);
                            }
                            else throw new InvalidOperationException("Illegal compressed instruction");
                            break;
                        case 0b100:
                            var c_11_10 = (instruction >> 10) & 0b11;
                            switch (c_11_10)
                            {
                                case 0b00: // c.srli
                                    opcode = opcodes.ALU_IMM;
                                    funct3 = funct3.SRLI;
                                    rs1 = 8 + (int)((instruction >> 7) & 0b111);
                                    rd = rs1;
                                    imm = (int)(((instruction >> 2) & 0b11111) | ((instruction >> 7) & 0b100000));
                                    break;
                                case 0b01: // c.srai
                                    opcode = opcodes.ALU_IMM;
                                    funct3 = funct3.SRAI;
                                    rs1 = 8 + (int)((instruction >> 7) & 0b111);
                                    rd = rs1;
                                    imm = (int)(((instruction >> 2) & 0b11111) | ((instruction >> 7) & 0b100000));
                                    break;
                                case 0b10: // c.andi
                                    opcode = opcodes.ALU_IMM;
                                    funct3 = funct3.ADDI;
                                    rs1 = 8 + (int)((instruction >> 7) & 0b111);
                                    rd = rs1;
                                    imm = (int)(((instruction >> 2) & 0b11111) | ((instruction >> 7) & 0b100000));
                                    imm = (int)((Reg64)imm).getSigned(6);
                                    break;
                                case 0b11:
                                    var c_6_5 = (instruction >> 5) & 0b11;
                                    switch (c_6_5)
                                    {
                                        case 0b00: // c.sub
                                            opcode = opcodes.ALU;
                                            funct3 = funct3.SUB;
                                            funct7 = funct7.SUB;
                                            rs1 = (int)((instruction >> 7) & 0b111);
                                            rs2 = (int)((instruction >> 2) & 0b111);
                                            rd = rs1;
                                            break;
                                        case 0b01: // c.xor
                                            opcode = opcodes.ALU;
                                            funct3 = funct3.XOR;
                                            rs1 = (int)((instruction >> 7) & 0b111);
                                            rs2 = (int)((instruction >> 2) & 0b111);
                                            rd = rs1;
                                            break;
                                        case 0b10: // c.or
                                            opcode = opcodes.ALU;
                                            funct3 = funct3.OR;
                                            rs1 = (int)((instruction >> 7) & 0b111);
                                            rs2 = (int)((instruction >> 2) & 0b111);
                                            rd = rs1;
                                            break;
                                        case 0b11: // c.and
                                            opcode = opcodes.ALU;
                                            funct3 = funct3.AND;
                                            rs1 = (int)((instruction >> 7) & 0b111);
                                            rs2 = (int)((instruction >> 2) & 0b111);
                                            rd = rs1;
                                            break;
                                    }
                                    break;
                            }
                            break;
                        case 0b101: // c.j
                            opcode = opcodes.JAL;
                            rd = 0x0;
                            imm = (int)(((instruction >> 2) & 0b1110) | ((instruction >> 7) & 0b10000) |
                                        ((instruction << 3) & 0b100000) | ((instruction >> 1) & 0b1000000) |
                                        ((instruction << 1) & 0b10000000) | ((instruction >> 1) & 0b1100000000) |
                                        ((instruction << 2) & 0b10000000000) | ((instruction >> 1) & 0b100000000000));
                            imm = (int)((Reg64)imm).getSigned(12);
                            break;
                        case 0b110: // c.beqz
                            opcode = opcodes.BRANCH;
                            funct3 = funct3.BEQ;
                            rs1 = (int)((instruction >> 1) & 0b111);
                            rd = 0x0;
                            imm = (int)(((instruction >> 2) & 0b110) | ((instruction >> 8) & 0b11000) |
                                        ((instruction << 3) & 0b100000) | ((instruction << 1) & 0b11000000) |
                                        ((instruction >> 4) & 0b100000000));
                            imm = (int)((Reg64)imm).getSigned(9);
                            break;
                        case 0b111: // c.bnez
                            opcode = opcodes.BRANCH;
                            funct3 = funct3.BNE;
                            rs1 = (int)((instruction >> 1) & 0b111);
                            rd = 0x0;
                            imm = (int)(((instruction >> 2) & 0b110) | ((instruction >> 8) & 0b11000) |
                                        ((instruction << 3) & 0b100000) | ((instruction << 1) & 0b11000000) |
                                        ((instruction >> 4) & 0b100000000));
                            imm = (int)((Reg64)imm).getSigned(9);
                            break;
                    }
                    break;
                case 0b10:
                    switch (c_funct3)
                    {
                        case 0b000: // c.slli
                            opcode = opcodes.ALU_IMM;
                            funct3 = funct3.SLLI;
                            rs1 = (int)((instruction >> 7) & 0b111);
                            rd = rs1;
                            imm = (int)(((instruction >> 2) & 0b11111) | ((instruction >> 7) & 0b100000));
                            break;
                        case 0b001: // c.slli64
                            throw new InvalidOperationException("c.slli64 not implemented");
                        case 0b100:
                            rs1 = (int)((instruction >> 7) & 0b111);
                            rs2 = (int)((instruction >> 2) & 0b111);
                            if ((instruction & 0b1000000000000) == 0)
                            {
                                if (rs2 == 0) // c.jr
                                {
                                    opcode = opcodes.JALR;
                                    rd = 0x0;
                                }
                                else // c.mv
                                {
                                    opcode = opcodes.ALU;
                                    funct3 = funct3.ADD;
                                    funct7 = funct7.ADD;
                                    rd = rs1;
                                    rs1 = 0x0;
                                }
                            }
                            else
                            {
                                if ((rs1 == 0) && (rs2 == 0)) // c.ebreak
                                {
                                    // To be implemented
                                }
                                else if (rs2 == 0) // c.jalr
                                {
                                    opcode = opcodes.JALR;
                                    rd = 0x1;
                                }
                                else // c.add
                                {
                                    opcode = opcodes.ALU;
                                    funct3 = funct3.ADD;
                                    funct7 = funct7.ADD;
                                    rd = rs1;
                                }
                            }
                            break;
                    }
                    break;
            }
        }
        else
        {

            opcode = (opcodes)(instruction & 0x7F);
            funct3 = (funct3)((instruction >> 12) & 0x7);
            rd = (int)((instruction >> 7) & 0x1F);
            rs1 = (int)((instruction >> 15) & 0x1F);
            rs2 = (int)((instruction >> 20) & 0x1F);
            funct7 = (funct7)((instruction >> 25) & 0x7F);
            shamt = (int)((instruction >> 20) & 0x1F);

            switch (opcode)
            {
                case opcodes.LUI:
                case opcodes.AUIPC:
                    imm = (int)(instruction >> 12);
                    break;
                case opcodes.JAL:
                    imm = (int)(((instruction >> 12) & 0x100000) | ((instruction >> 20) & 0x7fe) | ((instruction >> 9) & 0x800) | (instruction & 0xff000));
                    break;
                case opcodes.LOAD:
                case opcodes.JALR:
                case opcodes.ALU_IMM:
                case opcodes.ALU_IMM_64:
                case opcodes.FENCE:
                case opcodes.SYSTEM:
                    imm = (int)((instruction >> 20) & 0xfff);
                    break;
                case opcodes.BRANCH:
                    imm = (int)(((instruction >> 19) & 0x1000) | ((instruction >> 20) & 0x7e0) | ((instruction >> 7) & 0x1e) | ((instruction << 4) & 0x800));
                    break;
                case opcodes.STORE:
                    imm = (int)(((instruction >> 7) & 0x1f) | ((instruction >> 20) & 0xfe0));
                    break;
                case opcodes.ALU:
                case opcodes.ATOMIC:
                    imm = 0; // R-type instructions do not have an immediate value
                    break;
                default:
                    throw new InvalidOperationException("Unknown opcode: " + "Unknown opcode: 0b" + Convert.ToString((int)opcode, 2).PadLeft(7, '0'));
            }
        }
    }

    public override string ToString()
    {
        return $"Instruction: {opcode}, Rd: {rd}, Rs1: {rs1}, Rs2: {rs2}, Imm: {imm:X16}, Funct3: {funct3}, Funct7: {funct7}, Shamt: {shamt}";
    }

    private UInt32 decodeCompressed(UInt32 instruction)
    {
        return instruction;
    }
}