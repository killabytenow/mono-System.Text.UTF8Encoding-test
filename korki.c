#include <stdio.h>
#include <stdlib.h>

int main(int argc, char **argv)
{
	//printf("struct DecoderFallbackExceptionTest\n");
	//printf("{\n");
	//printf("	public bool valid, unknown_surrogate;\n");
	//printf("	public string str;\n");
	//printf("	public int index_fail;\n");
	//printf("	public DecoderFallbackExceptionTest (\n");
	//printf("			bool valid,\n");
	//printf("			string str,\n");
	//printf("			int index_fail, bool unknown_surrogate)\n");
	//printf("	{\n");
	//printf("		this.valid = valid;\n");
	//printf("		this.str = str;\n");
	//printf("		this.index_fail = index_fail;\n");
	//printf("		this.unknown_surrogate = unknown_surrogate;\n");
	//printf("	}\n");
	//printf("}\n");
	int c, mode;

	mode = 0;
	while((c = getchar()) != EOF)
	{
	      if(c == ':') {
		mode++;
		continue;
	      }
	      if(c == '\n')
		mode = -1;
	      switch(mode)
	      {
		case -1:
			printf(" } }),\n");
			mode++;
			break;
		case 0:
			printf("new DecoderFallbackExceptionTest ( ");
			mode++;
		case 1:
			putchar(c);
			break;
		case 2:
			printf(", \"");
			mode++;
		case 3:
			putchar(c);
			break;
		case 4:
			printf("\", new byte [] { 0x%02x", c);
			mode++;
			break;
		case 5:
			printf(", 0x%02x", c);
			break;
		default:
			fprintf(stderr, "Unknown mode %d.", mode);
			exit(1);
	      }
	}
	exit(0);
}
