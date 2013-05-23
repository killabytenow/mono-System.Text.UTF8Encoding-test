MONOPATH = ~/local
MCS = $(MONOPATH)/bin/mcs
GMCS = $(MONOPATH)/bin/gmcs
UTF16_SAMPLES = samples/utf16-all-good-before-dc00.txt \
		samples/utf16-all-good-after-dfff.txt  \
		samples/utf16-all-bad-dc00-dfff.txt    \
		samples/utf16-all-surrogates.txt

PROGS = convert.cs SomeTests.cs
BIN = $(PROGS:%.cs=%.exe) $(PROGS:%.cs=%.1.0.exe)
MDB = $(BIN:%.exe=%.mdb)

all : gen-utf16 $(BIN) $(UTF16_SAMPLES)

gen-utf16 : gen-utf16.c
	echo "$(BIN)"
	gcc -Wall -o gen-utf16 gen-utf16.c

%.exe : %.cs
	$(GMCS) -debug -unsafe $^ -sdk:2 -out:$@

%.1.0.exe : %.cs
	$(MCS) -debug -unsafe $^ -out:$@

SomeTests.1.0.exe : SomeTests.cs

SomeTests.exe : SomeTests.cs

convert.1.0.exe : convert.cs

convert.exe : convert.cs

$(UTF16_SAMPLES) : gen-utf16
	./gen-utf16

clean :
	rm -f -- $(UTF16_SAMPLES) gen-utf16 $(BIN) $(MDB)

