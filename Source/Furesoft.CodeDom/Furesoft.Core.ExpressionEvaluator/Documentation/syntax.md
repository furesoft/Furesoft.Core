# Syntax

## Variables
```
name = value;
```

## Function Definition
```
name(arg1, arg2) = arg1 * arg2;
```

## Function Call
```
name(1, 2 * 3);
```

```
f(x = 1, y = 2 * 3);
```

## Function Argument Constraint
```
name: arg1 is N 0 < x < 100;
```

```
g: x in N [5, INFINITY];
```

## Custom Set
```
set P in N = 1 < x && x % 1 == 0 && x % x == 0;
```

```
set custom = {1, 2, 3, 4};
```

## Alias
```
alias round as rnd;
```

## Use Modules
```
use geometry.*;
```
```
use formulars;
```
```
use "lib.math";
```

## Function Call With Module
```
geometry.planes.circumference(1);
```

## Module Definition In Different File
```
module trignonomic; 

b(p) = 2 * PI / p; b(PI);
```

## Absolute Value
```
|-42|;
```

## Expotential
```
2 ^ 4;
```

## Factorial
```
5!;
```

## Boolean
```
TRUE
```

```
FALSE
```

## Matrix
```
[1, 2, 3, 4, 5]
```

