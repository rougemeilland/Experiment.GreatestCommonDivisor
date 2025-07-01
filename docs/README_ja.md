# `BigInteger.GreatestCommonDivisor()` の高速化の試み

[**日本語**] | [[English](../README.md)]

## 1. 概要

.NET で多倍長整数の最大公約数を求める場合には通常 `BigInteger` 構造体の `GreatestCommonDivisor()` メソッドを使用することが多い。
.NET 9.0 現在ではこのメソッドはユークリッドの互除法を使用して実装されている。

互除法では剰余演算が不可欠であるが、多倍長整数においては剰余演算は CPU の負荷が高い。

本稿では、CPU の負荷をより軽減するために、互除法以外のアルゴリズムの実装を試みた。

## 2. 最大公約数を求めるアルゴリズムについて

ユークリッドの互除法のアルゴリズムは広く知られているため、本稿では言及しない。

本稿で実装を試みたアルゴリズムは
[準数値算法 算術演算](https://www.amazon.co.jp/%E6%BA%96%E6%95%B0%E5%80%A4%E7%AE%97%E6%B3%95%E2%80%95%E7%AE%97%E8%A1%93%E6%BC%94%E7%AE%97-art-computer-programming-4/dp/4781904262) (D. E. Knuth 著)
にて紹介されているものに .NET 固有のビット操作 (`BitOperations.TrailingZeroCount(uint)`) を使用した改良を加えたものである。
このアルゴリズムでは **乗算/除算/剰余演算を必要としない**。

<!-- 原語版=> [The Art of Computer Programming Volume 2 - Seminumerical algorithms](https://en.wikipedia.org/wiki/The_Art_of_Computer_Programming) (D. E. Knuth 著) "Chapter 4 – Arithmetic" -->

アルゴリズムの説明のために、例として `uint` 型の整数の最大公約数を求めるサンプルプログラムを以下に示す。

```c#
public static uint GreatestCommonDivisor(uint u, uint v)
{
    if (u == 0)
    {
        if (v == 0)
        {
            // If both u and v are 0, the greatest common divisor is undefined.
            throw new ArgumentException($"Both {nameof(u)} and {nameof(v)} are 0. The greatest common divisor of 0 and 0 is undefined.");
        }

        // If u is 0, then the greatest common divisor is equal to v
        return v;
    }

    if (v == 0)
    {
        // If v is 0, then the greatest common divisor is equal to 0
        return u;
    }

    // Both u and v are non-zero.
    System.Diagnostics.Debug.Assert(u > 0 && v > 0);

    // Find the largest integer k such that both u and v are divisible by 2^k.
    // Then divide u and v by 2^k.
    var k = 0;
    {
        var zeroBitCount = Math.Min(BitOperations.TrailingZeroCount(u), BitOperations.TrailingZeroCount(v));
        u >>= zeroBitCount;
        v >>= zeroBitCount;
        k += zeroBitCount;
    }

    // Either u or v is odd.
    System.Diagnostics.Debug.Assert(u > 0 && v > 0 && ((u & 1) != 0 || (v & 1) != 0));

    u >>= BitOperations.TrailingZeroCount(u); // If it is even, shift it right until it becomes odd.
    v >>= BitOperations.TrailingZeroCount(v); // If it is even, shift it right until it becomes odd.

    while (true)
    {
        // Both u and v are odd.
        System.Diagnostics.Debug.Assert(u > 0 && v > 0 && (u & 1) != 0 && (v & 1) != 0);

        // If u == v , return u * 2^k as the greatest common divisor.
        if (u == v)
            return u << k;

        // If u < v, swap the values ​​of u and v so that u > v.
        if (u < v)
            (v, u) = (u, v);

        // Both u and v are odd and u > v.
        System.Diagnostics.Debug.Assert(u > 0 && v > 0 && u > v && (u & 1) != 0 && (v & 1) != 0);

        // Subtract v from u, so that u is always a positive even number.
        u -= v;

        // u is an even number other than 0
        System.Diagnostics.Debug.Assert(u > 0 && (u & 1) == 0);

        u >>= BitOperations.TrailingZeroCount(u); // If it is even, shift it right until it becomes odd.
    }
}
```

このアルゴリズムは、GCD の以下の性質を利用して、u および v を小さくしていきながら繰り返す。
1. 自然数 w が 自然数 u と 自然数 v の公約数であるとき、GCD(u / w, v / w) * w == GCD(u, v)
2. 自然数 w が 自然数 u の因数であるが 自然数 v の因数ではないとき、 GCD(u / w, v) == GCD(u, v)
3. 自然数 u および v が u > v のとき、 GCD(u - v, v) == GCD(u, v)
4. 自然数 u が u > 0 のとき、 GCD(u, u) == u

## 3. 性能測定環境

### [使用したコード]
#### .NET 版
- [https://github.com/dotnet/runtime/blob/main/src/libraries/System.Runtime.Numerics/src/System/Numerics/BigIntegerCalculator.GcdInv.cs](https://github.com/dotnet/runtime/blob/main/src/libraries/System.Runtime.Numerics/src/System/Numerics/BigIntegerCalculator.GcdInv.cs)

#### 本稿で使用した修正版
- [https://github.com/rougemeilland/Experiment.GreatestCommonDivisor/blob/main/Experiment.CUI/BigIntegerCalculatorVer2.GcdInv.cs](https://github.com/rougemeilland/Experiment.GreatestCommonDivisor/blob/main/Experiment.CUI/BigIntegerCalculatorVer2.GcdInv.cs)

### [.NET runtime]
- .NET 9.0

### [OS]
- Windows 10 64bit

### [CPU]
- Intel Core i7-7700K

## 4. 測定方法

それそれのビット長の乱数を 8 種類生成したものの組み合わせで GCD を計算し、その計算時間の平均をとった。

## 5. 性能の比較

.NET での性能と、前述のアルゴリズムを使用した修正版の性能の比較結果を以下に示す。

| データの<br/>ビット数 | .NETの場合 [msec] | 本稿の修正版 [msec] |
|--:|--:|--:|
| 8 | 0.000000 | 0.000000 |
| 16 | 0.000000 | 0.000000 |
| 32 | 0.000100 | 0.000000 |
| 64 | 0.000200 | 0.000100 |
| 128 | 0.002000 | 0.000900 |
| 256 | 0.004400 | 0.003100 |
| 512 | 0.009800 | 0.009300 |
| 1024 | 0.022800 | 0.029800 |

## 6.考察

32 ビット以下のデータについては、計算に必要な時間が非常に短時間であったため、計測不能であった。

64 ビット以上のデータでは修正版アルゴリズムは優勢ではあったものの、更にビット数が増えるにつれて .NET版 (つまりユークリッドの互除法) が優勢となった。

修正版でのボトルネックとなる箇所を Visual Studio のパフォーマンスプロファイラで調べてみたところ、計算時間のネックとなっていたのは **減算** と **右シフト** であった。
これらの箇所をポインタなどの `unsafe` コードを使用して書き換えてみたが、計算時間はほぼ変わらなかった。

本稿で使用したアルゴリズムは剰余演算を使用していないため確かに高速ではあるが、しかしその代わりに集束が遅いようである。
そのため、1024bit 程度以上のより桁数の長い整数ではかえって不利となるようである。
