# Implementing `Fixed<T>`
Notes regarding the implementation details of the fixed point numeric type

## Implementing the conversion operators
As a numeric data type, it is natural for `Fixed<T>` to support conversion from and to the basic .NET value types such as `int` or `double`. Additionally, as it is a generic class, conversions between different precision types might be useful. This section outlines the challenges which these conversions provide and how they were dealt with.

### Conversion of basic value types
Integers are organically convertible to `Fixed<T>` simply because they are used as parameters when declaring an instance of `Fixed<T>`. Note that depending on the size of the parameter and the desired decimal precision, overflow / underflow may occur on assignment. This behaviour is not in any way detected; instead, the user is responsible for passing only acceptable values to `Fixed<T>` or handling overflow themselves. While this may seem like an unnecessary inconvenience, it is a trade-off prioritizing speed of assignment, which proves more valuable in most scenarios. (In fact, all versions of .NET make the same trade-off with `int` itself.)

In addition to the above, a conversion operator from `int` is also provided for ease of writing assignments and arithmetic operation. For simplicity, and sice such a conversion should rarely - if ever - occur in unintended scenarios, this conversion operator was made as an implicit one.

Conversion from an arbitrary `double` number to `Fixed<T>` is not available at this time. 

Representing decimal numbers, having a conversion to `double` is an obvious choice. Unlike the previous conversions hower, this one must be declared explicitly. The reason for this disparity is twofold. First, such a conversion may result in rounding errors, altering the original value. Second, as mentioned above, since a `double` value cannot be converted back to the original, implicit conversions may have unwanted consequences. 

No conversion operator is implemented from `Fixed<T>` to `int`, or any other numeric type, as such a conversion can be easily obtained with an in-between cast to `double`.

### Conversion between precisions
Converting from one fixed point precision to another presents a different challenge entirely. It seems like such a conversion could be implemented with another operator, probably an explicit one, as we don't want to transparently mix conversions. However, there is a major problem with this approach.

The whole implementation of `Fixed<T>` heavily relies on the concept of generic types, which allow for simple future extensibility and avoidance of unnecessary duplicate code. This approach does not lend itself to using operators, which don't support the concept of genericity. To use conversion operators, we would need to provide one for every possible combination of precisions, which is both prone to error and unmaintainable in the long term. 

In face of such predicament, a generic conversion method `ConvertTo<R>()` was implemented instead. All present and future `Q` derivatives can use this method to convert between each other, making it a suitable solution for this problem. And as we would want the conversion to be explicit anyway, this approach isn't much less usable than an operator.

## Implementing operations and interfaces
One advantage of fixed point numbers is the ease with which both arithmetic and comparison logic is implemented. Barring some bit shifting, all basic operations are the same with fixed point numbers as with integers. It is no surprise then that `Fixed<T>` implements all of these operations.

The arithmetic operators `+`, `-`, `*`, and `/` simply invoke the provided arithmetic methods: `Add`, `Subtract`, `Multiply`, and `Divide` respectively. By default, these only work on two `Fixed<T>` values of the same precision. In addition, using the implicit integer conversion operator mentioned above, all the operations also work between `Fixed<T>` and `int`.  

The type implements both `IEquatable<Fixed<T>>` and `IComparable<Fixed<T>>`, meaning it provides both the `Equals(Fixed<T>)` and the `CompareTo(Fixed<T>)` methods. These are complemented by all the comparison operators, namely `==, !=, <, >, <=, >=`. Similar to the above, these functions work on two `Fixed<T>` values of the same precision and on a `Fixed<T>` value and an integer.

`Fixed<T>` also overloads the standard object methods, `Equals(object)`, `GetHashCode()` and `ToString()`. These all work mainly as expected, with `ToString()` returning a `double` representation of the value. It should be noted that `Equals(object)` always returns `false` if the presented argument isn't of the same type. In particular, this means only `Fixed<T>` of the same precision can ever succeed in an equality test. An equal value represented with a different precision or as a `double` will compare as not equal in this method.

# Benchmarking `Fixed<T>`
Notes on measuring the performance of the fixed point numeric type.

## Setup
All tests were built in Release mode and run in the following configuration:

``` ini
BenchmarkDotNet=v0.11.4, OS=linuxmint 19.1
Intel Core i7-7700HQ CPU 2.80GHz (Kaby Lake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=2.2.105
  [Host]     : .NET Core 2.2.3 (CoreCLR 4.6.27414.05, CoreFX 4.6.27414.05), 64bit RyuJIT
  DefaultJob : .NET Core 2.2.3 (CoreCLR 4.6.27414.05, CoreFX 4.6.27414.05), 64bit RyuJIT
```

## Tests
Two main methods were used to benchmark the type. First, a method performing Gauss elimination on a matrix; second, a matrix multiplication method. In addition to these two, methods that simply repeat a single arithmetic operation were added as well. These tests are run for each supported precision of `Fixed<T>` as well as for `double` with identical implementation. 

### Inputs
Square arrays with each side of length were chosen as inputs for the main benchmarking methods, which was viewed as sufficient to provide significant results. All input arrays are pseudo-randomly generated and have identical content for each type. Since the implementation of the Gauss elimination method only works on regular matrices, a regular matrix is prepared for this test. In addition, the elimination test is run on two matrices; one filled with integers and the other with non-integer numbers.

For the arithmetic tests, each operation was repeated 10000 times. While repetition in these benchmarks would normally not be necessary, a single operation on `double` takes so little time that the benchmarks refuse to measure it.

## Results
The benchmarks returned the following results:

### Gauss elimination
Type | Mean | Error | StdDev | Input type
-----|-----:|------:|-------:|------------
Fixed<Q8_24> | 45.29 us | 0.4606 us | 0.4083 us | Integer
Fixed<Q8_24> | 46.76 us | 0.5717 us | 0.4774 us | Non-integer
Fixed<Q16_16>| 46.99 us | 0.2961 us | 0.2624 us | Integer
Fixed<Q16_16>| 45.80 us | 0.8493 us | 0.8341 us | Non-integer
Fixed<Q24_8> | 46.90 us | 0.3285 us | 0.3073 us | Integer
Fixed<Q24_8> | 45.35 us | 0.4323 us | 0.3832 us | Non-integer 
_Fixed\<T\> average_| 46.18 us |      | 0.4831 us |
double       | 7.527 us | 0.0390 us | 0.0346 us | Integer
double       | 7.508 us | 0.0195 us | 0.0163 us | Non-integer

### Matrix multiplication
Type | Mean | Error | StdDev |
-----|-----:|------:|-------:|
Fixed<Q8_24> | 121.76 us | 1.2557 us | 1.1131 us |
Fixed<Q16_16>| 121.41 us | 0.8951 us | 0.7935 us |
Fixed<Q24_8> | 132.47 us | 1.9186 us | 1.7947 us |
_Fixed\<T\> average_| 125.21 us |      | 1.3025 us |
double       | 21.609 us | 0.2532 us | 0.2369 us |

### Arithmetic operations
Type | Method | Mean | Error | StdDev |
-----|--------|-----:|------:|-------:|
Fixed<Q8_24>|       RepeatedAddition | 45.83 us | 0.6212 us | 0.5811 us |
Fixed<Q8_24>|    RepeatedSubtraction | 45.79 us | 0.3379 us | 0.3161 us |
Fixed<Q8_24>| RepeatedMultiplication | 81.20 us | 0.3261 us | 0.2723 us |
Fixed<Q8_24>|       RepeatedDivision | 156.49 us | 1.9477 us | 1.8219 us |
Fixed<Q16_16>|       RepeatedAddition | 45.49 us | 0.2418 us | 0.2144 us |
Fixed<Q16_16>|    RepeatedSubtraction | 45.91 us | 0.5038 us | 0.4712 us |
Fixed<Q16_16>| RepeatedMultiplication | 82.28 us | 0.9221 us | 0.8625 us |
Fixed<Q16_16>|       RepeatedDivision | 157.03 us | 1.3555 us | 1.2679 us |
Fixed<Q24_8> |       RepeatedAddition | 45.40 us | 0.2264 us | 0.1890 us |
Fixed<Q24_8> |    RepeatedSubtraction | 45.47 us | 0.1078 us | 0.0900 us |
Fixed<Q24_8> | RepeatedMultiplication | 81.42 us | 0.2819 us | 0.2354 us |
Fixed<Q24_8> |       RepeatedDivision | 155.92 us | 0.2527 us | 0.1973 us |
_Fixed\<T\> average_| RepeatedAddition | 45.57 us |           | 0.3739 us |
_Fixed\<T\> average_| RepeatedSubtraction | 45.72 us |        | 0.3317 us |
_Fixed\<T\> average_| RepeatedMultiplication | 81.63 us  |    | 0.5396 us |
_Fixed\<T\> average_| RepeatedDivision       | 156.48 us |    | 1.2866 us |
double |       RepeatedAddition | 10.712 us | 0.1600 us | 0.1418 us |
double |    RepeatedSubtraction | 10.709 us | 0.1618 us | 0.1514 us |
double | RepeatedMultiplication | 10.678 us | 0.0949 us | 0.0841 us |
double |       RepeatedDivision | 35.984 us | 0.6904 us | 0.7674 us |

### Comparison of benchmarks
The following table shows the coefficients by which the `Fixed<T>` implementations are slower than their `double` counterparts; that is, the ratio between the `Fixed<T>` average time and the `double` average time. 

Benchmark | Coefficient |
-------|-------------:
Gauss Elimination      | 6.14
Matrix Multiplication  | 5.79
Repeated Addition      | 4.25
Repeated Subtraction   | 4.27
Repeated Multiplication| 7.64
Repeated Division      | 4.35 

### Conclusions
These test results provide us with the following conclusions.

#### Speed is independent of precision type
With a single exception, every fixed test lies within two standard deviations from the average. These results indicate that the speed of operations is not dependent on where we put the decimal point. This is something we should suspect, knowing that the implementation uses simply shifted integers.

#### Speed is independent of whether the values are integers
We can see from the Gauss elimination test that non-integer inputs provide pretty much the same results as their integer counterparts. This again shouldn't surprise us, as all underlying operations work with integers regardless of what value they represent. 

#### `Fixed<T>` is reasonably fast
While the chosen test suite is by no means fully conclusive, all the tests suggest that its operational speed is within the same order of magnitude as `double`. Though more optimized versions could undoubtedly achieve better results in the future, this is a promising start.







