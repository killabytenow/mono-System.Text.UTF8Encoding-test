MONOPATH = ~/local
NUNIT = $(MONOPATH)/bin/nunit-console
MCS = $(MONOPATH)/bin/mcs
GMCS = $(MONOPATH)/bin/gmcs
UTF16_SAMPLES = samples/utf16-all-good-before-dc00.txt \
		samples/utf16-all-good-after-dfff.txt  \
		samples/utf16-all-bad-dc00-dfff.txt    \
		samples/utf16-all-surrogates.txt
BIN = convert.exe SomeTests.dll
MDB = $(BIN:%=%.mdb)

all : gen-utf16 $(BIN) $(UTF16_SAMPLES)

gen-utf16 : gen-utf16.c
	echo "$(BIN)"
	gcc -Wall -o gen-utf16 gen-utf16.c

%.exe : %.cs
	$(GMCS) -debug -unsafe $^ -sdk:2 -out:$@

%.dll : %.cs
	$(MCS) -target:library -unsafe -r:nunit.framework.dll -out:SomeTests.dll SomeTests.cs

SomeTests.dll : SomeTests.cs

convert.exe : convert.cs

run-test : SomeTests.dll
	$(NUNIT) SomeTests.dll

$(UTF16_SAMPLES) : gen-utf16
	./gen-utf16

clean :
	rm -f -- $(UTF16_SAMPLES) gen-utf16 $(BIN) $(MDB)

