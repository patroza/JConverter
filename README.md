# JConverter

## What does it do?

- Accept one or more SPSS files as input (Drag and drop onto the executable, or specify manually as commandline parameters)
- Output:
 - Mplus compatible .dat file(s)
 - Mplus compatible .inp file(s)


## Conversion details

### General

- If the first row contains alphabetical characters, then it is assumed to be the variable headers.
  If any other row contains alphabetical characters, an error is raised, as Mplus does not support this.
- Each line must have an equal amount of columns. Otherwise an error is raised.
- The default analysis type is BASIC.
- Output files (.dat and .inp) should not exist already. Otherwise an error is raised.
- Spaces in output file names are replaced by underscores (_)

### Conversion for the .dat file

- Replace empty values by -999
- Replace dot (.) by comma (,)
- Remove variable headers row

### Conversion for the .inp file

- If there are too long variable names (> 8), adds a comment to the top listing them
- If there are variable names that are not unique, adds a comment to the top listing them
- Specifies DATA: FILE IS x.dat;
- Specifies VARIABLE: NAMES ARE ...;
  IDVARIABLE IS ...;
  and MISSING ARE ALL (...);
- Specifies ANALYSIS: TYPE IS ...;
- Splits up long lines over multiple lines
