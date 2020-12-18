.section .text
.intel_syntax noprefix
.global main
main:
jmp _V4initv_rx

.extern _V17internal_allocatex_rPh

.global _V16decimal_additiondd_rd
_V16decimal_additiondd_rd:
addsd xmm0, xmm1
ret

.global _V19decimal_subtractiondd_rd
_V19decimal_subtractiondd_rd:
subsd xmm0, xmm1
ret

.global _V22decimal_multiplicationdd_rd
_V22decimal_multiplicationdd_rd:
mulsd xmm0, xmm1
ret

.global _V16decimal_divisiondd_rd
_V16decimal_divisiondd_rd:
divsd xmm0, xmm1
ret

.global _V22decimal_operator_orderdd_rd
_V22decimal_operator_orderdd_rd:
movsd xmm2, xmm1
mulsd xmm2, xmm0
movsd xmm3, xmm0
addsd xmm3, xmm2
divsd xmm1, xmm0
subsd xmm3, xmm1
movsd xmm0, xmm3
ret

.global _V30decimal_addition_with_constantd_rd
_V30decimal_addition_with_constantd_rd:
movsd xmm1, qword ptr [rip+_V30decimal_addition_with_constantd_rd_C0]
addsd xmm1, xmm0
movsd xmm0, xmm1
ret

.global _V33decimal_subtraction_with_constantd_rd
_V33decimal_subtraction_with_constantd_rd:
movsd xmm1, qword ptr [rip+_V33decimal_subtraction_with_constantd_rd_C0]
addsd xmm1, xmm0
movsd xmm0, xmm1
ret

.global _V36decimal_multiplication_with_constantd_rd
_V36decimal_multiplication_with_constantd_rd:
movsd xmm1, qword ptr [rip+_V36decimal_multiplication_with_constantd_rd_C0]
mulsd xmm0, xmm1
ret

.global _V30decimal_division_with_constantd_rd
_V30decimal_division_with_constantd_rd:
movsd xmm1, qword ptr [rip+_V30decimal_division_with_constantd_rd_C0]
divsd xmm1, xmm0
movsd xmm0, qword ptr [rip+_V30decimal_division_with_constantd_rd_C1]
divsd xmm1, xmm0
movsd xmm0, xmm1
ret

.global _V4initv_rx
_V4initv_rx:
sub rsp, 40
mov rax, 1
add rsp, 40
ret
pxor xmm0, xmm0
pxor xmm1, xmm1
call _V16decimal_additiondd_rd
pxor xmm0, xmm0
pxor xmm1, xmm1
call _V19decimal_subtractiondd_rd
pxor xmm0, xmm0
pxor xmm1, xmm1
call _V22decimal_multiplicationdd_rd
movsd xmm0, qword ptr [rip+_V4initv_rx_C0]
movsd xmm1, qword ptr [rip+_V4initv_rx_C0]
call _V16decimal_divisiondd_rd
movsd xmm0, qword ptr [rip+_V4initv_rx_C0]
movsd xmm1, qword ptr [rip+_V4initv_rx_C0]
call _V22decimal_operator_orderdd_rd
pxor xmm0, xmm0
call _V30decimal_addition_with_constantd_rd
pxor xmm0, xmm0
call _V33decimal_subtraction_with_constantd_rd
pxor xmm0, xmm0
call _V36decimal_multiplication_with_constantd_rd
pxor xmm0, xmm0
call _V30decimal_division_with_constantd_rd
ret

.section .data

.balign 16
_V30decimal_addition_with_constantd_rd_C0:
.byte 57, 180, 200, 118, 190, 159, 6, 64 # 2.828
.balign 16
_V33decimal_subtraction_with_constantd_rd_C0:
.byte 57, 180, 200, 118, 190, 159, 6, 192 # -2.828
.balign 16
_V36decimal_multiplication_with_constantd_rd_C0:
.byte 43, 13, 252, 168, 134, 253, 255, 63 # 1.9993959999999997
.balign 16
_V30decimal_division_with_constantd_rd_C0:
.byte 0, 0, 0, 0, 0, 0, 0, 64 # 2.0
.balign 16
_V30decimal_division_with_constantd_rd_C1:
.byte 57, 180, 200, 118, 190, 159, 246, 63 # 1.414
.balign 16
_V4initv_rx_C0:
.byte 0, 0, 0, 0, 0, 0, 240, 63 # 1.0

