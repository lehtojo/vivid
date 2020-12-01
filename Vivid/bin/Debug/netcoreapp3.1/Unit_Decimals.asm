section .text
global main
main:
jmp _V4initv_rx

extern _V17internal_allocatex_rPh

global _V16decimal_additiondd_rd
export _V16decimal_additiondd_rd
_V16decimal_additiondd_rd:
addsd xmm0, xmm1
ret

global _V19decimal_subtractiondd_rd
export _V19decimal_subtractiondd_rd
_V19decimal_subtractiondd_rd:
subsd xmm0, xmm1
ret

global _V22decimal_multiplicationdd_rd
export _V22decimal_multiplicationdd_rd
_V22decimal_multiplicationdd_rd:
mulsd xmm0, xmm1
ret

global _V16decimal_divisiondd_rd
export _V16decimal_divisiondd_rd
_V16decimal_divisiondd_rd:
divsd xmm0, xmm1
ret

global _V22decimal_operator_orderdd_rd
export _V22decimal_operator_orderdd_rd
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
export _V30decimal_addition_with_constantd_rd
_V30decimal_addition_with_constantd_rd:
movsd xmm1, qword [rel _V30decimal_addition_with_constantd_rd_C0]
addsd xmm1, xmm0
movsd xmm0, xmm1
ret

global _V33decimal_subtraction_with_constantd_rd
export _V33decimal_subtraction_with_constantd_rd
_V33decimal_subtraction_with_constantd_rd:
movsd xmm1, qword [rel _V33decimal_subtraction_with_constantd_rd_C0]
addsd xmm1, xmm0
movsd xmm0, xmm1
ret

global _V36decimal_multiplication_with_constantd_rd
export _V36decimal_multiplication_with_constantd_rd
_V36decimal_multiplication_with_constantd_rd:
movsd xmm1, qword [rel _V36decimal_multiplication_with_constantd_rd_C0]
mulsd xmm0, xmm1
ret

global _V30decimal_division_with_constantd_rd
export _V30decimal_division_with_constantd_rd
_V30decimal_division_with_constantd_rd:
movsd xmm1, qword [rel _V30decimal_division_with_constantd_rd_C0]
divsd xmm1, xmm0
movsd xmm0, qword [rel _V30decimal_division_with_constantd_rd_C1]
divsd xmm1, xmm0
movsd xmm0, xmm1
ret

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
movsd xmm0, qword [rel _V4initv_rx_C0]
movsd xmm1, qword [rel _V4initv_rx_C0]
call _V16decimal_divisiondd_rd
movsd xmm0, qword [rel _V4initv_rx_C0]
movsd xmm1, qword [rel _V4initv_rx_C0]
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

_V8allocatex_rPh:
push rbx
push rsi
sub rsp, 40
mov r8, [rel _VN10Allocation_current]
test r8, r8
je _V8allocatex_rPh_L0
mov rdx, [r8+16]
lea r9, [rdx+rcx]
cmp r9, 1000000
jg _V8allocatex_rPh_L0
lea r9, [rdx+rcx]
mov qword [r8+16], r9
lea r9, [rdx+rcx]
mov rax, [r8+8]
add rax, rdx
add rsp, 40
pop rsi
pop rbx
ret
_V8allocatex_rPh_L0:
mov rbx, rcx
mov rcx, 1000000
call _V17internal_allocatex_rPh
mov rcx, 24
mov rsi, rax
call _V17internal_allocatex_rPh
mov qword [rax+8], rsi
mov qword [rax+16], rbx
mov qword [rel _VN10Allocation_current], rax
mov rax, rsi
add rsp, 40
pop rsi
pop rbx
ret

_V8inheritsPhPS__rx:
push rbx
push rsi
sub rsp, 16
mov r8, [rcx]
mov r9, [rdx]
movzx r10, byte [r9]
xor rax, rax
_V8inheritsPhPS__rx_L1:
_V8inheritsPhPS__rx_L0:
movzx rcx, byte [r8+rax]
add rax, 1
cmp rcx, r10
jnz _V8inheritsPhPS__rx_L4
mov r11, rcx
mov rbx, 1
_V8inheritsPhPS__rx_L7:
_V8inheritsPhPS__rx_L6:
movzx r11, byte [r8+rax]
movzx rsi, byte [r9+rbx]
add rax, 1
add rbx, 1
cmp r11, rsi
jz _V8inheritsPhPS__rx_L9
cmp r11, 1
jne _V8inheritsPhPS__rx_L9
test rsi, rsi
jne _V8inheritsPhPS__rx_L9
mov rax, 1
add rsp, 16
pop rsi
pop rbx
ret
_V8inheritsPhPS__rx_L9:
jmp _V8inheritsPhPS__rx_L6
_V8inheritsPhPS__rx_L8:
jmp _V8inheritsPhPS__rx_L3
_V8inheritsPhPS__rx_L4:
cmp rcx, 2
jne _V8inheritsPhPS__rx_L3
xor rax, rax
add rsp, 16
pop rsi
pop rbx
ret
_V8inheritsPhPS__rx_L3:
jmp _V8inheritsPhPS__rx_L0
_V8inheritsPhPS__rx_L2:
add rsp, 16
pop rsi
pop rbx
ret

section .data

_VN10Allocation_current dq 0

_VN4Page_configuration:
dq _VN4Page_descriptor

_VN4Page_descriptor:
dq _VN4Page_descriptor_0
dd 24
dd 0

_VN4Page_descriptor_0:
db 'Page', 0, 1, 2, 0

_VN10Allocation_configuration:
dq _VN10Allocation_descriptor

_VN10Allocation_descriptor:
dq _VN10Allocation_descriptor_0
dd 8
dd 0

_VN10Allocation_descriptor_0:
db 'Allocation', 0, 1, 2, 0

align 16
_V30decimal_addition_with_constantd_rd_C0 db 57, 180, 200, 118, 190, 159, 6, 64 ; 2.828
align 16
_V33decimal_subtraction_with_constantd_rd_C0 db 57, 180, 200, 118, 190, 159, 6, 192 ; -2.828
align 16
_V36decimal_multiplication_with_constantd_rd_C0 db 43, 13, 252, 168, 134, 253, 255, 63 ; 1.9993959999999997
align 16
_V30decimal_division_with_constantd_rd_C0 db 0, 0, 0, 0, 0, 0, 0, 64 ; 2.0
align 16
_V30decimal_division_with_constantd_rd_C1 db 57, 180, 200, 118, 190, 159, 246, 63 ; 1.414
align 16
_V4initv_rx_C0 db 0, 0, 0, 0, 0, 0, 240, 63 ; 1.0