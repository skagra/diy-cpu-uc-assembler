include ../build-conf/vars.mk
include ../build-conf/rules.mk

.PHONY: all test

all: bin ucode test 

bin: 
	mkdir $(BIN)

ucode: mModeDecoder.txt mOpDecoder.txt uCode.txt uCtrl.txt uOps.txt 
	dotnet run --project ../microasm/microasm.csproj 4 1 1 1 . bin >$(BIN)/log.txt

test: ucode
	dotnet run --project ../cmp/cmp.csproj reference/mModeDecoder.bin $(BIN)/mModeDecoder.bin
	dotnet run --project ../cmp/cmp.csproj reference/mOpDecoder.bin $(BIN)/mOpDecoder.bin
	dotnet run --project ../cmp/cmp.csproj reference/uROM-0.bin $(BIN)/uROM-0.bin
	dotnet run --project ../cmp/cmp.csproj reference/uROM-1.bin $(BIN)/uROM-1.bin
	dotnet run --project ../cmp/cmp.csproj reference/uROM-2.bin $(BIN)/uROM-2.bin
	dotnet run --project ../cmp/cmp.csproj reference/uROM-3.bin $(BIN)/uROM-3.bin
	dotnet run --project ../cmp/cmp.csproj reference/uROM-4.bin $(BIN)/uROM-4.bin



