using System;

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

    SLLIW = 0b001,
    SRLIW = 0b101,
    SRAIW = 0b101,


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
}

public enum funct7
{
    ADD = 0b0000000,
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



public struct Instruction
{
    public InstructionType type;
    public opcodes opcode;
    public int rd;
    public int rs1;
    public int rs2;
    public int imm;
    public funct3 funct3;
    public funct7 funct7;
    public int shamt;

    public Instruction(UInt32 instruction)
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
                type = InstructionType.U;
                imm = (int)(instruction >> 12);
                break;
            case opcodes.JAL:
                type = InstructionType.J;
                imm = (int)(((instruction >> 11) & 0x10000) | ((instruction >> 20) & 0x7fe) | ((instruction >> 9) & 0x800) | (instruction & 0xff000));
                break;
            case opcodes.LOAD:
            case opcodes.JALR:
            case opcodes.ALU_IMM:
            case opcodes.FENCE:
            case opcodes.SYSTEM:
                type = InstructionType.I;
                imm = (int)((instruction >> 20) & 0xfff);
                break;
            case opcodes.BRANCH:
                type = InstructionType.B;
                imm = (int)(((instruction >> 19) & 0x800) | ((instruction << 4) & 0x400) | ((instruction >> 20) & 0x3e) | ((instruction >> 7) & 0x1e));
                break;
            case opcodes.STORE:
                type = InstructionType.S;
                imm = (int)(((instruction >> 7) & 0x1f) | ((instruction >> 20) & 0xfe0));
                break;
            case opcodes.ALU:
                type = InstructionType.R;
                imm = 0; // R-type instructions do not have an immediate value
                break;
            default:
                throw new InvalidOperationException("Unknown opcode: " + opcode);
        }
    }

    public override string ToString()
    {
        return $"Instruction: {opcode}, Type: {type}, Rd: {rd}, Rs1: {rs1}, Rs2: {rs2}, Imm: {imm}, Funct3: {funct3}, Funct7: {funct7}, Shamt: {shamt}";
    }
}