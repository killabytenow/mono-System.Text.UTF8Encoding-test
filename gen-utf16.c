/* this file builds some very big text files with
 * a lot of good and bad UTF-16 sequences. */

#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <errno.h>
#include <unistd.h>

unsigned int utf16 (unsigned int c)
{
	if (c > 0x10FFFF) {
		fprintf (stderr, "Bad char 0x%08x. Aborting.\n", c);
		exit (1);
	}
	if (c >= 0xDC00 && c <= 0xDFFF)
		fprintf (stderr, "Bad char 0x%08x.\n", c);
	if (c < 0xD800)
		return c;
	if (c > 0xDFFF)
		return c;
	c -= 0x10000;
	return (((c >> 10) | 0xD800) << 16) | ((c & 0x3FF) | 0xDC00);
}

void pc (char c, int f)
{
	write (f, &c, 1);
}

void printchar (FILE *file, unsigned int c)
{
	char temp [255], *t;
	int f = fileno (file);
	sprintf (temp, "UTF[0x%08x] = <", c);
	for (t = temp; *t; t++) {
		pc ('\0', f);
		pc (*t, f);
	}
	c = utf16 (c);
	if (c & 0xffff0000) {
		pc ((c >> 8*2) & 0xff, f);
		pc ((c >> 8*3) & 0xff, f);
	}
	pc ((c >> 8*1) & 0xff, f);
	pc ((c >> 8*0) & 0xff, f);
	pc (0, f); pc ('>', f);
	pc (0, f); pc ('\n', f);
}

int main (int argc, char **argv)
{
	struct segments {
		char *filename;
		unsigned int start;
		unsigned int end;
	} tests [] = {
		{ "samples/utf16-all-good-before-dc00.txt",  0x0000, 0xdc00 - 1 },
		{ "samples/utf16-all-good-after-dfff.txt",   0xe000, 0xffff     },
		{ "samples/utf16-all-bad-dc00-dfff.txt",     0xdc00, 0xdfff     },
		{ "samples/utf16-all-surrogates.txt",       0x10000, 0x10ffff   },
		{ NULL, 0, 0 }
	}, *t;
	unsigned c;
	FILE *f;

	for (t = tests; t->filename; t++)
	{
		if((f = fopen(t->filename, "w")) == NULL) {
			fprintf (stderr, "Cannot write file '%s': %s",
					t->filename, strerror (errno));
			exit (1);
		}
		for (c = t->start; c <= t->end; c++)
			printchar (f, c);
		fclose (f);
	}

	exit (0);
}

