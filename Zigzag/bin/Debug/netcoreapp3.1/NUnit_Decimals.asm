section .text
global function_run
extern allocate
extern integer_power
extern sys_print
extern sys_read
extern copy
extern offset_copy
extern deallocate

global function_decimal_addition
export function_decimal_addition
function_decimal_addition:
addsd xmm0, xmm1
ret

global function_decimal_subtraction
export function_decimal_subtraction
function_decimal_subtraction:
subsd xmm0, xmm1
ret

global function_decimal_multiplication
export function_decimal_multiplication
function_decimal_multiplication:
mulsd xmm0, xmm1
ret

global function_decimal_division
export function_decimal_division
function_decimal_division:
divsd xmm0, xmm1
ret

global function_decimal_operator_order
export function_decimal_operator_order
function_decimal_operator_order:
movsd xmm2, xmm1
mulsd xmm2, xmm0
movsd xmm3, xmm0
addsd xmm3, xmm2
divsd xmm1, xmm0
subsd xmm3, xmm1
movsd xmm0, xmm3
ret

global function_decimal_addition_with_constant
export function_decimal_addition_with_constant
function_decimal_addition_with_constant:
movsd xmm1, qword [rel function_decimal_addition_with_constant_C0]
addsd xmm1, xmm0
movsd xmm0, qword [rel function_decimal_addition_with_constant_C0]
addsd xmm1, xmm0
movsd xmm0, xmm1
ret

global function_decimal_subtraction_with_constant
export function_decimal_subtraction_with_constant
function_decimal_subtraction_with_constant:
movsd xmm1, qword [rel function_decimal_subtraction_with_constant_C0]
addsd xmm1, xmm0
movsd xmm0, qword [rel function_decimal_subtraction_with_constant_C1]
subsd xmm1, xmm0
movsd xmm0, xmm1
ret

global function_decimal_multiplication_with_constant
export function_decimal_multiplication_with_constant
function_decimal_multiplication_with_constant:
movsd xmm1, qword [rel function_decimal_multiplication_with_constant_C0]
mulsd xmm1, xmm0
movsd xmm0, qword [rel function_decimal_multiplication_with_constant_C0]
mulsd xmm1, xmm0
movsd xmm0, xmm1
ret

global function_decimal_division_with_constant
export function_decimal_division_with_constant
function_decimal_division_with_constant:
movsd xmm1, qword [rel function_decimal_division_with_constant_C0]
divsd xmm1, xmm0
movsd xmm0, qword [rel function_decimal_division_with_constant_C1]
divsd xmm1, xmm0
movsd xmm0, xmm1
ret

function_run:
sub rsp, 40
mov rax, 1
add rsp, 40
ret
movsd xmm0, qword [rel function_run_C0]
movsd xmm1, qword [rel function_run_C0]
call function_decimal_addition
movsd xmm0, qword [rel function_run_C0]
movsd xmm1, qword [rel function_run_C0]
call function_decimal_subtraction
movsd xmm0, qword [rel function_run_C0]
movsd xmm1, qword [rel function_run_C0]
call function_decimal_multiplication
movsd xmm0, qword [rel function_run_C1]
movsd xmm1, qword [rel function_run_C1]
call function_decimal_division
movsd xmm0, qword [rel function_run_C1]
movsd xmm1, qword [rel function_run_C1]
call function_decimal_operator_order
movsd xmm0, qword [rel function_run_C0]
call function_decimal_addition_with_constant
movsd xmm0, qword [rel function_run_C0]
call function_decimal_subtraction_with_constant
movsd xmm0, qword [rel function_run_C0]
call function_decimal_multiplication_with_constant
movsd xmm0, qword [rel function_run_C0]
call function_decimal_division_with_constant
ret

section .data

function_decimal_addition_with_constant_C0 dq 1.414
function_decimal_subtraction_with_constant_C0 dq -1.414
function_decimal_subtraction_with_constant_C1 dq 1.414
function_decimal_multiplication_with_constant_C0 dq 1.414
function_decimal_division_with_constant_C0 dq 2.0
function_decimal_division_with_constant_C1 dq 1.414
function_run_C0 dq 0.0
function_run_C1 dq 1.0