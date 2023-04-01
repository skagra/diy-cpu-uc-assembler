# DIY CPU Microcode Assembler

A μcode assembler for the [DIY CPU](https://github.com/skagra/diy-cpu-meta).

The assembler processes symbolic μcode definitions to create the following binary ROM files:

* `mModeDecoder.bin` - Addressing mode decoding.  A mapping from machine-code opcodes to the address of the μcode that implements the associated addressing mode.
* `mOpDecoder.bin` - Instruction execution decoding.  A mapping from machine-code opcodes to the address of the μcode that implements the operation.
* `uROM-*.bin` - Binary ROM files containing the actual μcode, sliced based on the ROM word width.

# Input Files

The μcode assembler takes the following files as input:

## mModeDecoder.txt

Each line in `mModeDecoder.txt` associates a symbolic name for an addressing mode with a set of numerical machine-code opcodes.  The symbolic name is used to identify the relevant μcode in `uCode.txt`. 

For [example](ucode/mModeDecoder.txt):

```
ACC         0A 4A 2A 6A  
IMM         69 29 C9 E0 C0 49 A9 A2 A0 09 E9      
...
```

## mOpDecoder.txt

Each line in `mOpDecoder.txt` associates a symbolic name for a machine-code opcode with a set of numerical machine-code opcodes.  The symbolic name is used to identify the relevant μcode in `uCode.txt`. 

For [example](ucode/mOpDecoder.txt):

```
ADC      69 65 75 6D 7D 79 61 71
SBC      E9 E5 F5 ED FD F9 E1 F1
...
```

## uCtrl.txt

Each line in `uCtrl.txt` associates a symbolic name for a control line with a binary "number" where each digit represents a control line with `.` => low and `1` => high.  These symbols are used to define μcode and further symbols in `uCode.txt` and `uOps.txt` respectively.

For [example](ucode/uCtrl.txt):

```
CDATA/LD/0      ........ ........ ........ .......1
CDATA/TO/CADDR  ........ ........ ........ ......1.
MEM/LD/XDATA    ........ ........ ........ .....1..
MEM/OUT/XDATA   ........ ........ ........ ....1...
...
```

## uOps.txt

Each line in `uOps.txt` associates a symbolic name with a binary *or* (`|`) of symbols from `uCtrl.txt` - that is of control lines.  This is a convenience mechanism used for abstraction and to reduce repetition when defining μcode in `uCode.txt`.     

For [example](ucode/uOps.txt):

```
MBR<-MEM        MEM/OUT/XDATA | MBR/LD/XDATA
MAR<-PC         PC/OUT/CADDR | MAR/LD/CADDR
PC-INC          PC/INC                         
```

## uCode.txt

`uCode.txt` contains the actual μcode defined in terms of symbols from in `mModeDecoder.txt`, `mOpDecoder.txt`, `uOps.txt` and `uCtrl.txt`.

Each line in `uCode.txt` is either:

* A *directive* - a line beginning `.`
* *μcode* - a binary *or* (`|`) of symbols from `uOps.txt` and `uCtrl.txt`.

The following directives are supported:

* `.label <name>` - A label which marks a location in the file that can be referenced elsewhere.  The following label *names* have a special meaning:
  * `RESET` - μcode which is executed when the CPU is reset.
  * `p0` - *Phase 0* (fetch) μcode which is executed before each machine code instruction.
* `.mode <name>` - Introduces μcode implementing a particular addressing mode (`p1`).  The `name` must be declared in `mModeDecoder.txt`. In this way μcode is shared by a set of machine-code instructions - for example the μcode of immediate mode addressing is identical regardless of the function of the machine code opcode.   
* `.opcode <name>` - Introduces μcode implementing a particular assembly language instruction (`p2`).  The name must be declared in `mOpDecoder.txt`.  In this way μcode is shared by a set of machine-code instructions - for example the μcode implementation `ADC` is identical regardless of addressing mode.

For [example](ucode/uCode.txt):

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

// Immediate mode addressing
.mode IMM
MAR<-PC | PC-INC | uP2                                               // Move the PC into MAR and jump to uP2

// Add with carry
.opcode ADC         
MBR<-MEM                                                            // Read memory into MBR
ALUB<-MBR                                                           // Move the value read into ALUB
ALUA<-A                                                             // Move the accumulator into ALUA
ALUOP-ADD | ALUR/OUT/CDATA | A/LD/CDATA | PZ/LD | Z/SRC/CDATA | uP0     // Add ALUA to ALB, store the result in the accumulator and set Z flag

// Branch is zero
.opcode BEQ                                                         
MBR<-MEM                                                            // Read the offset from memory at MAR into MBR
uZJMP BEQ_TRUE                                                      // Z flag is set jump to BEQ_TRUE
uP0                                                                 // Jump to uP0 - no branch needed
.label BEQ_TRUE
MBR/OUT/CDATA | PC/REL/CDATA | uP0      

...
```

# Command Line

The μcode assembler takes the following arguments:

* `ctrl-lines-bytes` - Width of control lines in byte.
* `ucode-addr-bytes` - Width of μcode address field in byte.
* `ucode-rom-word-size-bytes` - Word size to use for μcode ROMs in bytes.
* `ucode-rom-addr-width-bytes` - Address size to use for μcode ROMs in bytes.
* `source-directory` - Directory containing the source files (`mModeDecoder.txt`, `mOpDecoder.txt`, `uCode.txt`, `uCtrl.txt` and `uOps.txt`).
* `output-directory` -  Directory to write the output files (`mModeDecoder.bin`, `mOpDecoder.bin` and `uROM-*.bin`).



