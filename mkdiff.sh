#!/bin/sh

MONO="cnvout-mono"
MNET="cnvout-other"
DIFF="cnvout-diff"
mkdir -p "$DIFF/"
for i in "$MONO/"*.txt; do
        F="`basename "$i"`"
        M="Checking file '$F'..."
        echo -n "$M\r"
        if [ \(                                 \
               ! -e "$DIFF/.ok-$F"              \
               -o "$MONO/$F" -nt "$DIFF/.ok-$F" \
               -o "$MNET/$F" -nt "$DIFF/.ok-$F" \
             \) -a \(                           \
               ! -e "$DIFF/$F"                  \
               -o "$MONO/$F" -nt "$DIFF/$F"     \
               -o "$MNET/$F" -nt "$DIFF/$F"     \
             \) ]; then
          rm -f -- "$DIFF/.ok-$F" "$DIFF/$F"
          if [ -e "$MONO/$F" -a -e "$MNET/$F" ]; then
            H1="`md5sum "$MONO/$F"`"
            H2="`md5sum "$MNET/$F"`"
            if [ "$H1" != "$H2" ]; then
              echo -n "`echo "$M" | sed 's#.# #g'`\r"
              echo "ok!" > "$DIFF/.ok-$F"
            else
              echo "Differences found in file '$F'."
              diff -ur "$MONO/$F" "$MNET/$F" > "$DIFF/$F"
            fi
          else
            echo "File '$F' does not exists on both platforms."
          fi
        else
          echo -n "`echo "$M" | sed 's#.# #g'`\r"
        fi
done

echo "Finished."
exit 0
