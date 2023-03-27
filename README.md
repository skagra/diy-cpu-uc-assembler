# DIY CPU Microcode Assembler

Microcode assembler for the [DIY CPU](https://github.com/skagra/diy-cpu-meta).

The assembler processes symbolic microcode definitions to create the following binary ROM files:

* `mModeDecoder.bin` - A mapping from machine op codes to the address of the micro-code that implements the appropriate addressing mode.
* `mOpDecoder.bin` - A mapping from machine op codes to the address of the micro-code that implements the appropriate operation.
* `uROM-*.bin` - Binary ROM files containing the actual microcode, sliced based on the ROM word width parameter.

## Input Files

### mModeDecoder.txt

Each line in the `mModeDecoder.txt` associates a symbolic name for an addressing mode with a set of numerical machine-code opcodes.  The symbolic name is used to tag the relevant microcode in `uCode.txt`. 

```
ACC         0A 4A 2A 6A  
IMM         69 29 C9 E0 C0 49 A9 A2 A0 09 E9      
...
```

### mOpDecoder.txt

Each line in the `mOpDecoder.txt` associates a symbolic name for a (machine code) assembly language opcode with a set of numerical machine-code opcodes.  The symbolic name is used to tag the relevant microcode in `uCode.txt`. 

```
ADC      69 65 75 6D 7D 79 61 71
SBC      E9 E5 F5 ED FD F9 E1 F1
...
```

### uCtrl.txt

Each line in `uCtrl.txt` associates a symbolic name with a binary "number" where each digit represents a control line with `.` => low and `1` => high.  These symbols are used to define micrcode in `uCode.txt` and `uCode.txt`.

```
CDATA/LD/0      ........ ........ ........ .......1
CDATA/TO/CADDR  ........ ........ ........ ......1.
MEM/LD/XDATA    ........ ........ ........ .....1..
MEM/OUT/XDATA   ........ ........ ........ ....1...
...
```

### uOps.txt

Each line in `uOps.txt` associates a symbolic name with a logical `OR` of symbols from `uCtrl.txt` - that is of control lines.  This is a convenience mechanism used for abstraction and to reduce repetition when defining microcode in `uCode.txt`.     

```
MBR<-MEM        MEM/OUT/XDATA | MBR/LD/XDATA
MAR<-PC         PC/OUT/CADDR | MAR/LD/CADDR
PC-INC          PC/INC                         
```

### uCode.txt

`uCode.txt` contains the actual micro-code and makes use of the symbols defined in `mModeDecoder.txt`, `mOpDecoder.txt`, `uIos.txt` and `uCtrl.txt`.

Each line `uCode.txt` is either:

* A directive - a line beginning `.`
* A line of microcode, which is a logical `OR` of symbols from `uOps.txt` and `uCtrl.txt`.

The following directives are supported:

* `.label <name>` - Defines a label which marks a location in the file can be referenced elsewhere.  The following label names have special meaning:
  * `RESET` - Microcode to execute when the CPU is reset.
  * `p0` - The Phase 0 (fetch) microcode which is executed before each machine code instruction.
* `.mode <name>` - Introduces microcode implementing a particular addressing mode (`p1`).  The `name` must be declared in `mModeDecoder.txt`. In this way  microcode is associated with, and shared by, a set of machine-code instructions.  
* `.opcode <name>` - Introduces microcode implementing a particular assembly language instruction (`p2`).  The name must be declared in `mOpDecoder.txt` - in this way the microcode is associated with the set of numerical machine-code opcodes that use it.

```
.label RESET
CDATA/LD/0 | CDATA/TO/CADDR | MBR/LD/CDATA | MAR/LD/CADDR | A/LD/CDATA | X/LD/CDATA | ALUA/LD/CDATA | ALUB/LD/CDATA | PC/LD/CDATA
uP0

...

.label p0
MAR<-PC                                                     // PC points to opcode to execute - move the PC into MAR
PC-INC                                                      // Increase the PC                        
IR<-MEM                                                     // Load the current op code from memory into the IR
uP1                                                         // Separate cycle as we don't have a handle on the op code for decoding until the previous cycle is completed

...

.mode IMM
MAR<-PC | PC-INC | uP2                                               // Move the PC into MAR and jump to uP2

.opcode ADC         
MBR<-MEM                                                            // Read memory into MBR
ALUB<-MBR                                                           // Move the value read into ALUB
ALUA<-A                                                             // Move the accumulator into ALUA
ALUOP-ADD | ALUR/OUT/CDATA | A/LD/CDATA | PZ/LD | Z/SRC/CDATA | uP0     // Add ALUA to ALB, store the result in the accumulator and set Z flag
...
```

# Command Line

* `ctrl-lines-bytes` - 
* `ucode-addr-bytes` - 
* `ucode-rom-word-size-bytes` - 
* `ucode-rom-addr-width-bytes` -
* `source-directory` -
* `output-directory` -
