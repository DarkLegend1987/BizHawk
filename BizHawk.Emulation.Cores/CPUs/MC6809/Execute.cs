using System;

namespace BizHawk.Emulation.Common.Components.MC6809
{
	public partial class MC6809
	{
		private ulong totalExecutedCycles;
		public ulong TotalExecutedCycles { get { return totalExecutedCycles; } set { totalExecutedCycles = value; } }

		private int EI_pending;
		private bool interrupts_enabled;

		// variables for executing instructions
		public int instr_pntr = 0;
		public ushort[] cur_instr;
		public int opcode;
		public bool halted;
		public bool stopped;
		public bool jammed;
		public int LY;

		public void FetchInstruction(byte opcode)
		{
			switch (opcode)
			{
				case 0x00: DIRECT_MEM(NEG);							break; // NEG				(Direct)
				case 0x01: ILLEGAL();								break; // ILLEGAL
				case 0x02: ILLEGAL();								break; // ILLEGAL
				case 0x03: DIRECT_MEM(COM);							break; // COM				(Direct)
				case 0x04: DIRECT_MEM(LSR);							break; // LSR				(Direct)
				case 0x05: ILLEGAL();								break; // ILLEGAL
				case 0x06: DIRECT_MEM(ROR);							break; // ROR				(Direct)
				case 0x07: DIRECT_MEM(ASR);							break; // ASR				(Direct)
				case 0x08: DIRECT_MEM(ASL);							break; // ASL , LSL			(Direct)
				case 0x09: DIRECT_MEM(ROL);							break; // ROL				(Direct)
				case 0x0A: DIRECT_MEM(DEC8);						break; // DEC				(Direct)
				case 0x0B: ILLEGAL();								break; // ILLEGAL
				case 0x0C: DIRECT_MEM(INC8);						break; // INC				(Direct)
				case 0x0D: DIRECT_MEM(TST);							break; // TST				(Direct)
				case 0x0E: JMP_DIR_();								break; // JMP				(Direct)
				case 0x0F: DIRECT_MEM(CLR);							break; // CLR				(Direct)
				case 0x10: PAGE_2();								break; // Page 2
				case 0x11: PAGE_3();								break; // Page 3
				case 0x12: NOP_();									break; // NOP				(Inherent)
				case 0x13: SYNC_();									break; // SYNC				(Inherent)
				case 0x14: ILLEGAL();								break; // ILLEGAL
				case 0x15: ILLEGAL();								break; // ILLEGAL
				case 0x16: LBR_(true);								break; // LBRA				(Relative)
				case 0x17: LBSR_();									break; // LBSR				(Relative)
				case 0x18: ILLEGAL();								break; // ILLEGAL
				case 0x19: REG_OP(DA, A);							break; // DAA				(Inherent)
				case 0x1A: REG_OP_IMD_CC(OR8);						break; // ORCC				(Immediate)
				case 0x1B: ILLEGAL();								break; // ILLEGAL
				case 0x1C: REG_OP_IMD_CC(AND8);						break; // ANDCC				(Immediate)
				case 0x1D: REG_OP(SEX, A);							break; // SEX				(Inherent)
				case 0x1E: EXG_();									break; // EXG				(Immediate)
				case 0x1F: TFR_();									break; // TFR				(Immediate)
				case 0x20: BR_(true);								break; // BRA				(Relative)
				case 0x21: BR_(false);								break; // BRN				(Relative)
				case 0x22: BR_(!(FlagC | FlagZ));					break; // BHI				(Relative)
				case 0x23: BR_(FlagC | FlagZ);						break; // BLS				(Relative)
				case 0x24: BR_(!FlagC);								break; // BHS , BCC			(Relative)
				case 0x25: BR_(FlagC);								break; // BLO , BCS			(Relative)
				case 0x26: BR_(!FlagZ);								break; // BNE				(Relative)
				case 0x27: BR_(FlagZ);								break; // BEQ				(Relative)
				case 0x28: BR_(!FlagV);								break; // BVC				(Relative)
				case 0x29: BR_(FlagV);								break; // BVS				(Relative)
				case 0x2A: BR_(!FlagN);								break; // BPL				(Relative)
				case 0x2B: BR_(FlagN);								break; // BMI				(Relative)
				case 0x2C: BR_(FlagN == FlagV);						break; // BGE				(Relative)
				case 0x2D: BR_(FlagN ^ FlagV);						break; // BLT				(Relative)
				case 0x2E: BR_((!FlagZ) & (FlagN == FlagV));		break; // BGT				(Relative)
				case 0x2F: BR_(FlagZ | (FlagN ^ FlagV));			break; // BLE				(Relative)
				case 0x30: JR_COND(!FlagC);							break; // LEAX				(Indexed)
				case 0x31: ;			break; // LEAY				(Indexed)
				case 0x32: ;					break; // LEAS				(Indexed)
				case 0x33: ;						break; // LEAU				(Indexed)
				case 0x34: ;							break; // PSHS				(Immediate)
				case 0x35: ;							break; // PULS				(Immediate)
				case 0x36: ;			break; // PSHU				(Immediate)
				case 0x37: ;							break; // PULU				(Immediate)
				case 0x38: ILLEGAL();								break; // ILLEGAL
				case 0x39: ;					break; // RTS				(Inherent)
				case 0x3A: ;				break; // ABX				(Inherent)
				case 0x3B: ;						break; // RTI				(Inherent)
				case 0x3C: ;							break; // CWAI				(Inherent)
				case 0x3D: ;							break; // MUL				(Inherent)
				case 0x3E: ILLEGAL();								break; // ILLEGAL
				case 0x3F: ;							break; // SWI				(Inherent)
				case 0x40: REG_OP(NEG, A);							break; // NEGA				(Inherent)
				case 0x41: ILLEGAL();								break; // ILLEGAL
				case 0x42: ILLEGAL();								break; // ILLEGAL
				case 0x43: REG_OP(COM, A);							break; // COMA				(Inherent)
				case 0x44: REG_OP(LSR, A);							break; // LSRA				(Inherent)
				case 0x45: ILLEGAL();								break; // ILLEGAL
				case 0x46: REG_OP(ROR, A);							break; // RORA				(Inherent)
				case 0x47: REG_OP(ASR, A);							break; // ASRA				(Inherent)
				case 0x48: REG_OP(ASL, A);							break; // ASLA , LSLA		(Inherent)
				case 0x49: REG_OP(ROL, A);							break; // ROLA				(Inherent)
				case 0x4A: REG_OP(DEC8, A);							break; // DECA				(Inherent)
				case 0x4B: ILLEGAL();								break; // ILLEGAL
				case 0x4C: REG_OP(INC8, A);							break; // INCA				(Inherent)
				case 0x4D: REG_OP(TST, A);							break; // TSTA				(Inherent)
				case 0x4E: ILLEGAL();								break; // ILLEGAL
				case 0x4F: REG_OP(CLR, A);							break; // CLRA				(Inherent)
				case 0x50: REG_OP(NEG, B);							break; // NEGB				(Inherent)
				case 0x51: ILLEGAL();								break; // ILLEGAL
				case 0x52: ILLEGAL();							    break; // ILLEGAL
				case 0x53: REG_OP(COM, B);							break; // COMB				(Inherent)
				case 0x54: REG_OP(LSR, B);							break; // LSRB				(Inherent)
				case 0x55: ILLEGAL();								break; // ILLEGAL
				case 0x56: REG_OP(ROR, B);							break; // RORB				(Inherent)
				case 0x57: REG_OP(ASR, B);							break; // ASRB				(Inherent)
				case 0x58: REG_OP(ASL, B);							break; // ASLB , LSLB		(Inherent)
				case 0x59: REG_OP(ROL, B);							break; // ROLB				(Inherent)
				case 0x5A: REG_OP(DEC8, B);							break; // DECB				(Inherent)
				case 0x5B: ILLEGAL();								break; // ILLEGAL
				case 0x5C: REG_OP(INC8, B);							break; // INCB				(Inherent)
				case 0x5D: REG_OP(TST, B);							break; // TSTB				(Inherent)
				case 0x5E: ILLEGAL();								break; // ILLEGAL
				case 0x5F: REG_OP(CLR, B);							break; // CLRB				(Inherent)
				case 0x60: REG_OP(TR, B);						break; // NEG				(Indexed)
				case 0x61: ILLEGAL();								break; // ILLEGAL
				case 0x62: ILLEGAL();								break; // ILLEGAL
				case 0x63: REG_OP(TR, B);						break; // COM				(Indexed)
				case 0x64: REG_OP(TR, B);						break; // LSR				(Indexed)
				case 0x65: ILLEGAL();								break; // ILLEGAL
				case 0x66: REG_OP(TR, B);					break; // ROR				(Indexed)
				case 0x67: REG_OP(TR, A);						break; // ASR				(Indexed)
				case 0x68: REG_OP(TR, A);						break; // ASL , LSL			(Indexed)
				case 0x69: REG_OP(TR, A);						break; // ROL				(Indexed)
				case 0x6A: REG_OP(TR, A);						break; // DEC				(Indexed)
				case 0x6B: ILLEGAL();								break; // ILLEGAL
				case 0x6C: REG_OP(TR, A);						break; // INC				(Indexed)
				case 0x6D: REG_OP(TR, A);						break; // TST				(Indexed)
				case 0x6E: REG_OP(TR, A);					break; // JMP				(Indexed)
				case 0x6F: REG_OP(TR, A);						break; // CLR				(Indexed)
				case 0x70: REG_OP(TR, A);						break; // NEG				(Extended)
				case 0x71: ILLEGAL();								break; // ILLEGAL
				case 0x72: ILLEGAL();								break; // ILLEGAL
				case 0x73: REG_OP(TR, A);						break; // COM				(Extended)
				case 0x74: REG_OP(TR, A);						break; // LSR				(Extended)
				case 0x75: ILLEGAL();								break; // ILLEGAL
				case 0x76: REG_OP(TR, A);					break; // ROR				(Extended)
				case 0x77: REG_OP(TR, A);						break; // ASR				(Extended)
				case 0x78: REG_OP(TR, A);						break; // ASL , LSL			(Extended)
				case 0x79: REG_OP(TR, A);						break; // ROL				(Extended)
				case 0x7A: REG_OP(TR, A);						break; // DEC				(Extended)
				case 0x7B: ILLEGAL();								break; // ILLEGAL
				case 0x7C: REG_OP(TR, A);						break; // INC				(Extended)
				case 0x7D: REG_OP(TR, A);						break; // TST				(Extended)
				case 0x7E: REG_OP(TR, A);					break; // JMP				(Extended)
				case 0x7F: REG_OP(TR, A);						break; // CLR				(Extended)
				case 0x80: REG_OP(ADD8, A);						break; // SUBA				(Immediate)
				case 0x81: REG_OP(ADD8, A);						break; // CMPA				(Immediate)
				case 0x82: REG_OP(ADD8, A);						break; // SBCA				(Immediate)
				case 0x83: REG_OP(ADD8, A);						break; // SUBD				(Immediate)
				case 0x84: REG_OP(ADD8, A);						break; // ANDA				(Immediate)
				case 0x85: REG_OP(ADD8, A);						break; // BITA				(Immediate)
				case 0x86: REG_OP(ADD8, A);				break; // LDA				(Immediate)
				case 0x87: ILLEGAL();								break; // ILLEGAL
				case 0x88: REG_OP(ADC8, A);						break; // EORA				(Immediate)
				case 0x89: REG_OP(ADC8, A);						break; // ADCA				(Immediate)
				case 0x8A: REG_OP(ADC8, A);						break; // ORA				(Immediate)
				case 0x8B: REG_OP(ADC8, A);						break; // ADDA				(Immediate)
				case 0x8C: REG_OP(ADC8, A);						break; // CMPX				(Immediate)
				case 0x8D: REG_OP(ADC8, A);						break; // BSR				(Relative)
				case 0x8E: REG_OP(ADC8, A);				break; // LDX				(Immediate)
				case 0x8F: ILLEGAL();								break; // ILLEGAL
				case 0x90: REG_OP(ADD8, A);						break; // SUBA				(Direct)
				case 0x91: REG_OP(ADD8, A);						break; // CMPA				(Direct)
				case 0x92: REG_OP(ADD8, A);						break; // SBCA				(Direct)
				case 0x93: REG_OP(ADD8, A);						break; // SUBD				(Direct)
				case 0x94: REG_OP(ADD8, A);						break; // ANDA				(Direct)
				case 0x95: REG_OP(ADD8, A);						break; // BITA				(Direct)
				case 0x96: REG_OP(ADD8, A);				break; // LDA				(Direct)
				case 0x97: REG_OP(ADD8, A);						break; // STA				(Direct)
				case 0x98: REG_OP(ADC8, A);						break; // EORA				(Direct)
				case 0x99: REG_OP(ADC8, A);						break; // ADCA				(Direct)
				case 0x9A: REG_OP(ADC8, A);						break; // ORA				(Direct)
				case 0x9B: REG_OP(ADC8, A);						break; // ADDA				(Direct)
				case 0x9C: REG_OP(ADC8, A);						break; // CMPX				(Direct)
				case 0x9D: REG_OP(ADC8, A);						break; // JSR				(Direct)
				case 0x9E: REG_OP(ADC8, A);				break; // LDX				(Direct)
				case 0x9F: REG_OP(ADC8, A);						break; // STX				(Direct)
				case 0xA0: REG_OP(AND8, A);						break; // SUBA				(Indexed)
				case 0xA1: REG_OP(AND8, A);						break; // CMPA				(Indexed)
				case 0xA2: REG_OP(AND8, A);						break; // SBCA				(Indexed)
				case 0xA3: REG_OP(AND8, A);						break; // SUBD				(Indexed)
				case 0xA4: REG_OP(AND8, A);						break; // ANDA				(Indexed)
				case 0xA5: REG_OP(AND8, A);						break; // BITA				(Indexed)
				case 0xA6: REG_OP(AND8, A);				break; // LDA				(Indexed)
				case 0xA7: REG_OP(AND8, A);						break; // STA				(Indexed)
				case 0xA8: REG_OP(XOR8, A);						break; // EORA				(Indexed)
				case 0xA9: REG_OP(XOR8, A);						break; // ADCA				(Indexed)
				case 0xAA: REG_OP(XOR8, A);						break; // ORA				(Indexed)
				case 0xAB: REG_OP(XOR8, A);						break; // ADDA				(Indexed)
				case 0xAC: REG_OP(XOR8, A);						break; // CMPX				(Indexed)
				case 0xAD: REG_OP(XOR8, A);						break; // JSR				(Indexed)
				case 0xAE: REG_OP(XOR8, A);				break; // LDX				(Indexed)
				case 0xAF: REG_OP(XOR8, A);						break; // STX				(Indexed)
				case 0xB0: REG_OP(AND8, A);						break; // SUBA				(Extended)
				case 0xB1: REG_OP(AND8, A);						break; // CMPA				(Extended)
				case 0xB2: REG_OP(AND8, A);						break; // SBCA				(Extended)
				case 0xB3: REG_OP(AND8, A);						break; // SUBD				(Extended)
				case 0xB4: REG_OP(AND8, A);						break; // ANDA				(Extended)
				case 0xB5: REG_OP(AND8, A);						break; // BITA				(Extended)
				case 0xB6: REG_OP(AND8, A);				break; // LDA				(Extended)
				case 0xB7: REG_OP(AND8, A);						break; // STA				(Extended)
				case 0xB8: REG_OP(XOR8, A);						break; // EORA				(Extended)
				case 0xB9: REG_OP(XOR8, A);						break; // ADCA				(Extended)
				case 0xBA: REG_OP(XOR8, A);						break; // ORA				(Extended)
				case 0xBB: REG_OP(XOR8, A);						break; // ADDA				(Extended)
				case 0xBC: REG_OP(XOR8, A);						break; // CMPX				(Extended)
				case 0xBD: REG_OP(XOR8, A);						break; // JSR				(Extended)
				case 0xBE: REG_OP(XOR8, A);				break; // LDX				(Extended)
				case 0xBF: REG_OP(XOR8, A);						break; // STX				(Extended)
				case 0xC0: REG_OP(ADD8, A);						break; // SUBB				(Immediate)
				case 0xC1: REG_OP(ADD8, A);						break; // CMPB				(Immediate)
				case 0xC2: REG_OP(ADD8, A);						break; // SBCB				(Immediate)
				case 0xC3: REG_OP(ADD8, A);						break; // ADDD				(Immediate)
				case 0xC4: REG_OP(ADD8, A);						break; // ANDB				(Immediate)
				case 0xC5: REG_OP(ADD8, A);						break; // BITB				(Immediate)
				case 0xC6: REG_OP(ADD8, A);				break; // LDB				(Immediate)
				case 0xC7: ILLEGAL();								break; // ILLEGAL
				case 0xC8: REG_OP(ADC8, A);						break; // EORB				(Immediate)
				case 0xC9: REG_OP(ADC8, A);						break; // ADCB				(Immediate)
				case 0xCA: REG_OP(ADC8, A);						break; // ORB				(Immediate)
				case 0xCB: REG_OP(ADC8, A);						break; // ADDB				(Immediate)
				case 0xCC: REG_OP(ADC8, A);						break; // LDD				(Immediate)
				case 0xCD: ILLEGAL();								break; // ILLEGAL
				case 0xCE: REG_OP(ADC8, A);				break; // LDU				(Immediate)
				case 0xCF: ILLEGAL();								break; // ILLEGAL
				case 0xD0: REG_OP(ADD8, B);						break; // SUBB				(Direct)
				case 0xD1: REG_OP(ADD8, B);						break; // CMPB				(Direct)
				case 0xD2: REG_OP(ADD8, B);						break; // SBCB				(Direct)
				case 0xD3: REG_OP(ADD8, B);						break; // ADDD				(Direct)
				case 0xD4: REG_OP(ADD8, B);						break; // ANDB				(Direct)
				case 0xD5: REG_OP(ADD8, B);						break; // BITB				(Direct)
				case 0xD6: REG_OP(ADD8, B);				break; // LDB				(Direct)
				case 0xD7: REG_OP(ADD8, B);						break; // STB				(Direct)
				case 0xD8: REG_OP(ADC8, B);						break; // EORB				(Direct)
				case 0xD9: REG_OP(ADC8, B);						break; // ADCB				(Direct)
				case 0xDA: REG_OP(ADC8, B);						break; // ORB				(Direct)
				case 0xDB: REG_OP(ADC8, B);						break; // ADDB				(Direct)
				case 0xDC: REG_OP(ADC8, B);						break; // LDD				(Direct)
				case 0xDD: REG_OP(ADC8, B);						break; // STD				(Direct)
				case 0xDE: REG_OP(ADC8, B);				break; // LDU				(Direct)
				case 0xDF: REG_OP(ADC8, B);						break; // STU				(Direct)
				case 0xE0: REG_OP(AND8, B);						break; // SUBB				(Indexed)
				case 0xE1: REG_OP(AND8, B);						break; // CMPB				(Indexed)
				case 0xE2: REG_OP(AND8, B);						break; // SBCB				(Indexed)
				case 0xE3: REG_OP(AND8, B);						break; // ADDD				(Indexed)
				case 0xE4: REG_OP(AND8, B);						break; // ANDB				(Indexed)
				case 0xE5: REG_OP(AND8, B);						break; // BITB				(Indexed)
				case 0xE6: REG_OP(AND8, B);				break; // LDB				(Indexed)
				case 0xE7: REG_OP(AND8, B);						break; // STB				(Indexed)
				case 0xE8: REG_OP(XOR8, B);						break; // EORB				(Indexed)
				case 0xE9: REG_OP(XOR8, B);						break; // ADCB				(Indexed)
				case 0xEA: REG_OP(XOR8, B);						break; // ORB				(Indexed)
				case 0xEB: REG_OP(XOR8, B);						break; // ADDB				(Indexed)
				case 0xEC: REG_OP(XOR8, B);						break; // LDD				(Indexed)
				case 0xED: REG_OP(XOR8, B);						break; // STD				(Indexed)
				case 0xEE: REG_OP(XOR8, B);				break; // LDU				(Indexed)
				case 0xEF: REG_OP(XOR8, B);						break; // STU				(Indexed)
				case 0xF0: REG_OP(AND8, B);						break; // SUBB				(Extended)
				case 0xF1: REG_OP(AND8, B);						break; // CMPB				(Extended)
				case 0xF2: REG_OP(AND8, B);						break; // SBCB				(Extended)
				case 0xF3: REG_OP(AND8, B);						break; // ADDD				(Extended)
				case 0xF4: REG_OP(AND8, B);						break; // ANDB				(Extended)
				case 0xF5: REG_OP(AND8, B);						break; // BITB				(Extended)
				case 0xF6: REG_OP(AND8, B);				break; // LDB				(Extended)
				case 0xF7: REG_OP(AND8, B);						break; // STB				(Extended)
				case 0xF8: REG_OP(XOR8, B);						break; // EORB				(Extended)
				case 0xF9: REG_OP(XOR8, B);						break; // ADCB				(Extended)
				case 0xFA: REG_OP(XOR8, B);						break; // ORB				(Extended)
				case 0xFB: REG_OP(XOR8, B);						break; // ADDB				(Extended)
				case 0xFC: REG_OP(XOR8, B);						break; // LDD				(Extended)
				case 0xFD: REG_OP(XOR8, B);						break; // STD				(Extended)
				case 0xFE: REG_OP(XOR8, B);				break; // LDU				(Extended)
				case 0xFF: REG_OP(XOR8, B);						break; // STU				(Extended)
			}
		}

		public void FetchInstruction2(byte opcode)
		{
			switch (opcode)
			{

				default: ILLEGAL(); break;
			}
		}

		public void FetchInstruction3(byte opcode)
		{
			switch (opcode)
			{

				default: ILLEGAL(); break;
			}
		}
	}
}