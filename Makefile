UTF16_SAMPLES = samples/utf16-all-good-before-dc00.txt \
		samples/utf16-all-good-after-dfff.txt  \
		samples/utf16-all-bad-dc00-dfff.txt    \
		samples/utf16-all-surrogates.txt

all : gen-utf16 convert.exe $(UTF16_SAMPLES)

gen-utf16 : gen-utf16.c
	gcc -Wall -o gen-utf16 gen-utf16.c

convert.exe :
	mcs -debug convert.cs -out:convert.exe

$(UTF16_SAMPLES) : gen-utf16
	./gen-utf16

clean :
	rm -f -- $(UTF16_SAMPLES) gen-utf16 convert.exe convert.exe.mdb

