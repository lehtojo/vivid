//analyze
.global _V22internal_integer_powerxx_rx
.global _V3powxx_rx
_V3powxx_rx:
_V22internal_integer_powerxx_rx:
// x0: base
// x1: exponent
mov x0, xzr
ret

.global _V3cosd_rd
_V3cosd_rd:
mov x0, xzr
ret

.global _V3sind_rd
_V3sind_rd:
mov x0, xzr
ret

.global _V4sqrtd_rd
_V4sqrtd_rd:
fsqrt d0, d0
ret

.global _V4sqrtd_rd
_V4sqrtx_rd:
scvtf d0, x0
fsqrt d0, d0
ret

// x^y = 2^(y*log2(x))
// x0: base
// x1: exponent
.global _V22internal_decimal_powerdd_rd
.global _V3powdd_rd
.global vivid_pow
vivid_pow:
_V3powdd_rd:
_V22internal_decimal_powerdd_rd:
fmov d4, d1
fcvtzs x0, d0
mov x4, xzr
_V22internal_decimal_powerdd_L0:
asr x0, x0, #1
add x4, x4, #1
cbnz x0, _V22internal_decimal_powerdd_L0
sub x4, x4, #1
mov x2, #1
lsl x3, x2, x4
scvtf d1, x3
fdiv d0, d0, d1
fmov d2, #1.000000
fcmp d0, d2
b.eq _V22internal_decimal_powerdd_L1
fmov d3, d0
mov x0, #0x28e5
movk x0, #0xf55f, lsl #16
movk x0, #0xc8b2, lsl #32
movk x0, #0xc00d, lsl #48
fmov d1, x0
mov x0, #0x1af2
movk x0, #0x4a5c, lsl #16
movk x0, #0x4e59, lsl #32
movk x0, #0x4024, lsl #48
fmov d2, x0
fmul d2, d2, d0
fadd d1, d2, d1
fmul d0, d0, d3
mov x0, #0xa521
movk x0, #0x2d78, lsl #16
movk x0, #0xf712, lsl #32
movk x0, #0xc02f, lsl #48
fmov d2, x0
fmul d2, d2, d0
fadd d1, d2, d1
fmul d0, d0, d3
mov x0, #0x08bf
movk x0, #0x6455, lsl #16
movk x0, #0xc349, lsl #32
movk x0, #0x4033, lsl #48
fmov d2, x0
fmul d2, d2, d0
fadd d1, d2, d1
fmul d0, d0, d3
mov x0, #0xd923
movk x0, #0xadcd, lsl #16
movk x0, #0xefd3, lsl #32
movk x0, #0xc031, lsl #48
fmov d2, x0
fmul d2, d2, d0
fadd d1, d2, d1
fmul d0, d0, d3
mov x0, #0xac8e
movk x0, #0x8a1e, lsl #16
movk x0, #0xad69, lsl #32
movk x0, #0x4027, lsl #48
fmov d2, x0
fmul d2, d2, d0
fadd d1, d2, d1
fmul d0, d0, d3
mov x0, #0xf008
movk x0, #0x5819, lsl #16
movk x0, #0x7af3, lsl #32
movk x0, #0xc016, lsl #48
fmov d2, x0
fmul d2, d2, d0
fadd d1, d2, d1
fmul d0, d0, d3
mov x0, #0xc6a7
movk x0, #0x5752, lsl #16
movk x0, #0xf007, lsl #32
movk x0, #0x3ffd, lsl #48
fmov d2, x0
fmul d2, d2, d0
fadd d1, d2, d1
fmul d0, d0, d3
mov x0, #0x9c8a
movk x0, #0xb022, lsl #16
movk x0, #0x8f59, lsl #32
movk x0, #0xbfda, lsl #48
fmov d2, x0
fmul d2, d2, d0
fadd d1, d2, d1
fmul d0, d0, d3
mov x0, #0x4b3e
movk x0, #0xe839, lsl #16
movk x0, #0x36d5, lsl #32
movk x0, #0x3fac, lsl #48
fmov d2, x0
fmul d2, d2, d0
fadd d1, d2, d1
fmul d0, d0, d3
mov x0, #0x6bab
movk x0, #0xc905, lsl #16
movk x0, #0x29de, lsl #32
movk x0, #0xbf6b, lsl #48
fmov d2, x0
fmul d2, d2, d0
fadd d0, d2, d1
scvtf d1, x4
fadd d0, d0, d1
b _V22internal_decimal_powerdd_L2
_V22internal_decimal_powerdd_L1:
scvtf d0, x4
_V22internal_decimal_powerdd_L2:
fmul d0, d0, d4
fcvtzs x0, d0
scvtf d1, x0
fsub d0, d0, d1
mov x1, #1
lsl x0, x1, x0
scvtf d4, x0
fmov d3, d0
fmov d1, #1.000000
mov x0, #0x3a50
movk x0, #0xfefa, lsl #16
movk x0, #0x2e42, lsl #32
movk x0, #0x3fe6, lsl #48
fmov d2, x0
fmul d2, d2, d0
fadd d1, d2, d1
fmul d0, d0, d3
mov x0, #0x7711
movk x0, #0xff83, lsl #16
movk x0, #0xbfbd, lsl #32
movk x0, #0x3fce, lsl #48
fmov d2, x0
fmul d2, d2, d0
fadd d1, d2, d1
fmul d0, d0, d3
mov x0, #0x92d1
movk x0, #0xd6c4, lsl #16
movk x0, #0x6b08, lsl #32
movk x0, #0x3fac, lsl #48
fmov d2, x0
fmul d2, d2, d0
fadd d1, d2, d1
fmul d0, d0, d3
mov x0, #0xfa3d
movk x0, #0x7901, lsl #16
movk x0, #0xb2ab, lsl #32
movk x0, #0x3f83, lsl #48
fmov d2, x0
fmul d2, d2, d0
fadd d1, d2, d1
fmul d0, d0, d3
mov x0, #0x7a2f
movk x0, #0x71bf, lsl #16
movk x0, #0xd87e, lsl #32
movk x0, #0x3f55, lsl #48
fmov d2, x0
fmul d2, d2, d0
fadd d1, d2, d1
fmul d0, d0, d3
mov x0, #0x8cd8
movk x0, #0xb554, lsl #16
movk x0, #0x30b4, lsl #32
movk x0, #0x3f24, lsl #48
fmov d2, x0
fmul d2, d2, d0
fadd d1, d2, d1
fmul d0, d0, d3
mov x0, #0xa615
movk x0, #0x01a7, lsl #16
movk x0, #0xf87e, lsl #32
movk x0, #0x3eef, lsl #48
fmov d2, x0
fmul d2, d2, d0
fadd d1, d2, d1
fmul d0, d0, d3
mov x0, #0x1e45
movk x0, #0xe37d, lsl #16
movk x0, #0x5559, lsl #32
movk x0, #0x3eb6, lsl #48
fmov d2, x0
fmul d2, d2, d0
fadd d1, d2, d1
fmul d0, d0, d3
mov x0, #0x4ca6
movk x0, #0x25f2, lsl #16
movk x0, #0x681a, lsl #32
movk x0, #0x3e79, lsl #48
fmov d2, x0
fmul d2, d2, d0
fadd d1, d2, d1
fmul d0, d0, d3
mov x0, #0x8040
movk x0, #0x2069, lsl #16
movk x0, #0x5bbe, lsl #32
movk x0, #0x3e45, lsl #48
fmov d2, x0
fmul d2, d2, d0
fadd d0, d2, d1
fmul d0, d4, d0
ret






















// x^y = 2^(y*log2(x))
// x0: base
// x1: exponent
.global _V26dev_internal_decimal_powerdd_rd
_V26dev_internal_decimal_powerdd_rd:
internal_decimal_power:
str d19, [sp, #-16]!
str x30, [sp, #8]
fmov d19, d1

// s = y * log(2, x)
bl internal_base_two_logarithm
fmul d0, d0, d19

fcvtzs x0, d0
scvtf d1, x0
fsub d0, d0, d1 // frac(s) = s - int(s)

// 2^s = 2^(int(s)+frac(s)) = 2^int(s) * 2^frac(s)
mov x1, #1
lsl x0, x1, x0 // 2^int(s)
scvtf d19, x0

// 2^frac(s)
bl internal_base_two_power_fraction

fmul d0, d19, d0 // 2^int(s) * 2^frac(s) = 2^s = 2^(y*log2(x)) = x^y

ldr x30, [sp, #8]
ldr d19, [sp], #16
ret

// log2(value) = s
// log2(2^s) = s
// 2^(x+y) = value
// 2^x * 2^y = value
// 2^y = value / 2^x

// d0: value
// 2^(x+y) = value where x is any positive integer and y is a decimal in interval ]1.0, 2.0[
.global _V27internal_base_two_logarithmd_rd
_V27internal_base_two_logarithmd_rd:
internal_base_two_logarithm:
stp x30, x19, [sp, #-16]!
fcvtzs x0, d0

// Divide as long as the value is not zero and record how many shifts it took
mov x19, xzr // x
internal_base_two_logarithm_L0:
asr x0, x0, #1
add x19, x19, #1
cbnz x0, internal_base_two_logarithm_L0
sub x19, x19, #1

mov x2, #1
lsl x3, x2, x19 // 2^x
scvtf d1, x3

// 2^(x+y) = value
// 2^x * 2^y = value
// 2^y = value / 2^x where y in ]1.0, 2.0[
fdiv d0, d0, d1 // 2^y

fmov d2, #1.000000
fcmp d0, d2
b.eq internal_base_two_logarithm_L1

// y = log2(2^y)
bl internal_base_two_logarithm_fraction

scvtf d1, x19
fadd d0, d0, d1 // log2(value) = x + y
ldp x30, x19, [sp], #16
ret

internal_base_two_logarithm_L1:
scvtf d0, x19
ldp x30, x19, [sp], #16
ret

// Approximates base two logarithm on interval [1.0, 2.0]
// Grade: 10
// Fitted to point list: (n, 2^n) where n: { 1.0, 1.01, 1.02, ..., 2.0 }
// Maximum error approximetely: +/- 1E-9
// Approximation: -0.003315863730831x¹⁰ + 0.055105862233765x⁹ - 0.414999410635979x⁸ + 1.87110075102502x⁷ - 5.6200689092475x⁶ + 11.838695827717199x⁵ - 17.9368237140235x⁴ + 19.762838621864599x³ - 15.982560559251x² + 10.153024982207601x - 3.7229975862184
// d0: value in [1.0, 2.0]
.global _V36internal_base_two_logarithm_fractiond_rd
_V36internal_base_two_logarithm_fractiond_rd:
internal_base_two_logarithm_fraction:
fmov d3, d0
//mov x0, #-3.7229975862184
mov x0, #0x28e5
movk x0, #0xf55f, lsl #16
movk x0, #0xc8b2, lsl #32
movk x0, #0xc00d, lsl #48
fmov d1, x0
//mov x0, #10.153024982207601
mov x0, #0x1af2
movk x0, #0x4a5c, lsl #16
movk x0, #0x4e59, lsl #32
movk x0, #0x4024, lsl #48
fmov d2, x0
// Somehow did not work: fnmadd d1, d2, d0, d1
fmul d2, d2, d0
fadd d1, d2, d1
fmul d0, d0, d3
//mov x0, #-15.982560559251
mov x0, #0xa521
movk x0, #0x2d78, lsl #16
movk x0, #0xf712, lsl #32
movk x0, #0xc02f, lsl #48
fmov d2, x0
fmul d2, d2, d0
fadd d1, d2, d1
fmul d0, d0, d3
//mov x0, #19.762838621864599
mov x0, #0x08bf
movk x0, #0x6455, lsl #16
movk x0, #0xc349, lsl #32
movk x0, #0x4033, lsl #48
fmov d2, x0
fmul d2, d2, d0
fadd d1, d2, d1
fmul d0, d0, d3
//mov x0, #-17.9368237140235
mov x0, #0xd923
movk x0, #0xadcd, lsl #16
movk x0, #0xefd3, lsl #32
movk x0, #0xc031, lsl #48
fmov d2, x0
fmul d2, d2, d0
fadd d1, d2, d1
fmul d0, d0, d3
//mov x0, #11.838695827717199
mov x0, #0xac8e
movk x0, #0x8a1e, lsl #16
movk x0, #0xad69, lsl #32
movk x0, #0x4027, lsl #48
fmov d2, x0
fmul d2, d2, d0
fadd d1, d2, d1
fmul d0, d0, d3
//mov x0, #-5.6200689092475
mov x0, #0xf008
movk x0, #0x5819, lsl #16
movk x0, #0x7af3, lsl #32
movk x0, #0xc016, lsl #48
fmov d2, x0
fmul d2, d2, d0
fadd d1, d2, d1
fmul d0, d0, d3
//mov x0, #1.87110075102502
mov x0, #0xc6a7
movk x0, #0x5752, lsl #16
movk x0, #0xf007, lsl #32
movk x0, #0x3ffd, lsl #48
fmov d2, x0
fmul d2, d2, d0
fadd d1, d2, d1
fmul d0, d0, d3
//mov x0, #-0.414999410635979
mov x0, #0x9c8a
movk x0, #0xb022, lsl #16
movk x0, #0x8f59, lsl #32
movk x0, #0xbfda, lsl #48
fmov d2, x0
fmul d2, d2, d0
fadd d1, d2, d1
fmul d0, d0, d3
//mov x0, #0.055105862233765
mov x0, #0x4b3e
movk x0, #0xe839, lsl #16
movk x0, #0x36d5, lsl #32
movk x0, #0x3fac, lsl #48
fmov d2, x0
fmul d2, d2, d0
fadd d1, d2, d1
fmul d0, d0, d3
//mov x0, #-0.003315863730831
mov x0, #0x6bab
movk x0, #0xc905, lsl #16
movk x0, #0x29de, lsl #32
movk x0, #0xbf6b, lsl #48
fmov d2, x0
fmul d2, d2, d0
fadd d0, d2, d1
ret

// Approximates power of base two on interval [0.0, 1.0]
// Fitted to point list: (n, 2^n) where n: { 0.0, 0.01, 0.02, ..., 1.0 }
// Maximum error less than: +/- 1E-13
// Approximation: 0.000000009945766x¹⁰ + 0.000000094647143x⁹ + 0.000001331174543x⁸ + 0.000015244804719x⁷ + 0.000154039439317x⁶ + 0.001333354454793x⁵ + 0.009618129377711x⁴ + 0.055504108635693x³ + 0.240226506960362x² + 0.693147180559956x + 1
// d0: value in [0.0, 1.0]
.global _V32internal_base_two_power_fractiond_rd
_V32internal_base_two_power_fractiond_rd:
internal_base_two_power_fraction:
fmov d3, d0
fmov d1, #1.000000
//mov x0, #0.693147180559956
mov x0, #0x3a50
movk x0, #0xfefa, lsl #16
movk x0, #0x2e42, lsl #32
movk x0, #0x3fe6, lsl #48
fmov d2, x0
// Somehow did not work: fmadd d1, d2, d0, d1
fmul d2, d2, d0
fadd d1, d2, d1
fmul d0, d0, d3
//mov x0, #0.240226506960362
mov x0, #0x7711
movk x0, #0xff83, lsl #16
movk x0, #0xbfbd, lsl #32
movk x0, #0x3fce, lsl #48
fmov d2, x0
fmul d2, d2, d0
fadd d1, d2, d1
fmul d0, d0, d3
//mov x0, #0.055504108635693
mov x0, #0x92d1
movk x0, #0xd6c4, lsl #16
movk x0, #0x6b08, lsl #32
movk x0, #0x3fac, lsl #48
fmov d2, x0
fmul d2, d2, d0
fadd d1, d2, d1
fmul d0, d0, d3
//mov x0, #0.009618129377711
mov x0, #0xfa3d
movk x0, #0x7901, lsl #16
movk x0, #0xb2ab, lsl #32
movk x0, #0x3f83, lsl #48
fmov d2, x0
fmul d2, d2, d0
fadd d1, d2, d1
fmul d0, d0, d3
//mov x0, #0.001333354454793
mov x0, #0x7a2f
movk x0, #0x71bf, lsl #16
movk x0, #0xd87e, lsl #32
movk x0, #0x3f55, lsl #48
fmov d2, x0
fmul d2, d2, d0
fadd d1, d2, d1
fmul d0, d0, d3
//mov x0, #0.000154039439317
mov x0, #0x8cd8
movk x0, #0xb554, lsl #16
movk x0, #0x30b4, lsl #32
movk x0, #0x3f24, lsl #48
fmov d2, x0
fmul d2, d2, d0
fadd d1, d2, d1
fmul d0, d0, d3
//mov x0, #0.000015244804719
mov x0, #0xa615
movk x0, #0x01a7, lsl #16
movk x0, #0xf87e, lsl #32
movk x0, #0x3eef, lsl #48
fmov d2, x0
fmul d2, d2, d0
fadd d1, d2, d1
fmul d0, d0, d3
//mov x0, #0.000001331174543
mov x0, #0x1e45
movk x0, #0xe37d, lsl #16
movk x0, #0x5559, lsl #32
movk x0, #0x3eb6, lsl #48
fmov d2, x0
fmul d2, d2, d0
fadd d1, d2, d1
fmul d0, d0, d3
//mov x0, #0.000000094647143
mov x0, #0x4ca6
movk x0, #0x25f2, lsl #16
movk x0, #0x681a, lsl #32
movk x0, #0x3e79, lsl #48
fmov d2, x0
fmul d2, d2, d0
fadd d1, d2, d1
fmul d0, d0, d3
//mov x0, #0.000000009945766
mov x0, #0x8040
movk x0, #0x2069, lsl #16
movk x0, #0x5bbe, lsl #32
movk x0, #0x3e45, lsl #48
fmov d2, x0
fmul d2, d2, d0
fadd d0, d2, d1
ret
