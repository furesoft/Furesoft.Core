# Macros

Macros are evaluated before the code. They can manipulate the root scope or transform source code.
Every macro has the ability to change it's behavior or set some settings over all macro-calls. 

The 'rulefor' macro defines a behaviorchange for a specific macro

Example:
```
rulefor(resolve, x + y -> x - y);
```

## rename

The rename macro can rename a function or variable in the root scope.

```
rename(oldname, newname);
```

Example:
```
rename(round(2), rndm);
```

The round function with 2 Arguments will now be called with 'rndm'

## unpackBinominal
```
unpackBinominal(expresison);
```

Example:
```
unpackBinominal((4 + 3) ^ 2);
```

## inverse
Calculate the inverse of a value

```
inverse(expresison);
```

Example:
```
inverse(2);
```

## displayTree
Display the expression tree in the console

shouldArgumentBeBinded can be 0 or 1 (FALSE or TRUE)

```
displayTree(expression, shouldArgumentBeBinded);
```

Example:
```
displayTree(2+2*3, FALSE);
```

## average
Calculate the average

expressions is a comma seperated list

```
average(expressions);
```

Example:
```
average(2, 1, 5, 4, 5);
```