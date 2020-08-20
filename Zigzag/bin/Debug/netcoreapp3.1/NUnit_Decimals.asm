section .text
global _start
_start:
call _V4initv_rx
mov rax, 60
xor rdi, rdi
syscall

extern _V8allocatex_rPh

global _V16decimal_additiondd_rd
_V16decimal_additiondd_rd:
addsd xmm0, xmm1
ret

global _V19decimal_subtractiondd_rd
_V19decimal_subtractiondd_rd:
subsd xmm0, xmm1
ret

global _V22decimal_multiplicationdd_rd
_V22decimal_multiplicationdd_rd:
mulsd xmm0, xmm1
ret

global _V16decimal_divisiondd_rd
_V16decimal_divisiondd_rd:
divsd xmm0, xmm1
ret

global _V22decimal_operator_orderdd_rd
_V22decimal_operator_orderdd_rd:
movsd xmm2, xmm1
mulsd xmm2, xmm0
movsd xmm3, xmm0
addsd xmm3, xmm2
divsd xmm1, xmm0
subsd xmm3, xmm1
movsd xmm0, xmm3
ret

global _V30decimal_addition_with_constantd_rd
_V30decimal_addition_with_constantd_rd:
movsd xmm1, qword [rel _V30decimal_addition_with_constantd_rd_C0]
addsd xmm1, xmm0
movsd xmm0, qword [rel _V30decimal_addition_with_constantd_rd_C0]
addsd xmm1, xmm0
movsd xmm0, xmm1
ret

global _V33decimal_subtraction_with_constantd_rd
_V33decimal_subtraction_with_constantd_rd:
movsd xmm1, qword [rel _V33decimal_subtraction_with_constantd_rd_C0]
addsd xmm1, xmm0
movsd xmm0, qword [rel _V33decimal_subtraction_with_constantd_rd_C1]
subsd xmm1, xmm0
movsd xmm0, xmm1
ret

global _V36decimal_multiplication_with_constantd_rd
_V36decimal_multiplication_with_constantd_rd:
movsd xmm1, qword [rel _V36decimal_multiplication_with_constantd_rd_C0]
mulsd xmm1, xmm0
movsd xmm0, qword [rel _V36decimal_multiplication_with_constantd_rd_C0]
mulsd xmm1, xmm0
movsd xmm0, xmm1
ret

global _V30decimal_division_with_constantd_rd
_V30decimal_division_with_constantd_rd:
movsd xmm1, qword [rel _V30decimal_division_with_constantd_rd_C0]
divsd xmm1, xmm0
movsd xmm0, qword [rel _V30decimal_division_with_constantd_rd_C1]
divsd xmm1, xmm0
movsd xmm0, xmm1
ret

_V4initv_rx:
sub rsp, 8
mov rax, 1
add rsp, 8
ret
movsd xmm0, qword [rel _V4initv_rx_C0]
movsd xmm1, qword [rel _V4initv_rx_C0]
call _V16decimal_additiondd_rd
movsd xmm0, qword [rel _V4initv_rx_C0]
movsd xmm1, qword [rel _V4initv_rx_C0]
call _V19decimal_subtractiondd_rd
movsd xmm0, qword [rel _V4initv_rx_C0]
movsd xmm1, qword [rel _V4initv_rx_C0]
call _V22decimal_multiplicationdd_rd
movsd xmm0, qword [rel _V4initv_rx_C1]
movsd xmm1, qword [rel _V4initv_rx_C1]
call _V16decimal_divisiondd_rd
movsd xmm0, qword [rel _V4initv_rx_C1]
movsd xmm1, qword [rel _V4initv_rx_C1]
call _V22decimal_operator_orderdd_rd
movsd xmm0, qword [rel _V4initv_rx_C0]
call _V30decimal_addition_with_constantd_rd
movsd xmm0, qword [rel _V4initv_rx_C0]
call _V33decimal_subtraction_with_constantd_rd
movsd xmm0, qword [rel _V4initv_rx_C0]
call _V36decimal_multiplication_with_constantd_rd
movsd xmm0, qword [rel _V4initv_rx_C0]
call _V30decimal_division_with_constantd_rd
ret

section .data

_V30decimal_addition_with_constantd_rd_C0 dq 1.414
_V33decimal_subtraction_with_constantd_rd_C0 dq -1.414
_V33decimal_subtraction_with_constantd_rd_C1 dq 1.414
_V36decimal_multiplication_with_constantd_rd_C0 dq 1.414
_V30decimal_division_with_constantd_rd_C0 dq 2.0
_V30decimal_division_with_constantd_rd_C1 dq 1.414
_V4initv_rx_C0 dq 0.0
_V4initv_rx_C1 dq 1.0