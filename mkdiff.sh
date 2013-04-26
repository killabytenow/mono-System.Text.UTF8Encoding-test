#!/bin/sh

MONO="cnvout-mono"
MNET="cnvout-other"
DIFF="cnvout-diff"
rm -rf "$DIFF/"
mkdir -p "$DIFF/"
for i in "$MONO/"*.txt; do
        F="`basename "$i"`"
        M="Checking file '$F'..."
        echo -n "$M\r"
        H1="`md5sum "$MONO/$F"`"
        H2="`md5sum "$MNET/$F"`"
        if [ "$H1" != "$H2" ]; then
          echo -n "`echo "$M" | sed 's#.# #g'`\r"
        else
          echo "Differences found in file '$F'."
          diff -ur "$MONO/$F" "$MNET/$F" > "$DIFF/$F"
        fi
done

echo "Finished."
exit 0
