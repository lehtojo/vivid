section .text
global function_run
extern allocate
extern integer_power
extern sys_print
extern sys_read
extern copy
extern offset_copy
extern deallocate

function_basic_calculation:
movsd xmm2, xmm0
addsd xmm2, xmm1
movsd xmm3, xmm0
mulsd xmm3, xmm1
movsd xmm1, qword [function_basic_calculation_C0]
addsd xmm1, xmm0
divsd xmm3, xmm1
subsd xmm2, xmm3
movsd xmm0, qword [function_basic_calculation_C1]
addsd xmm2, xmm0
movsd xmm0, xmm2
ret

function_decimal_array_test:
sub rsp, 40
movsd xmm3, xmm0
addsd xmm3, xmm2
cvtsd2si rcx, xmm3
sal rcx, 3
movsd qword [rsp+48], xmm0
movsd qword [rsp+56], xmm1
movsd qword [rsp+64], xmm2
call allocate
movsd xmm0, qword [rsp+56]
movsd xmm1, xmm0
movsd xmm2, qword [rsp+64]
addsd xmm1, xmm2
cvtsd2si rcx, [rsp+48]
movsd qword [rax+rcx*8], xmm1
movsd xmm1, qword [function_decimal_array_test_C0]
addsd xmm0, xmm1
cvtsd2si rcx, xmm0
movsd xmm1, qword [rax+rcx*8]
addsd xmm1, xmm2
movsd xmm0, xmm1
add rsp, 40
ret

function_call_test_end:
cvtsi2sd xmm1, rcx
addsd xmm1, xmm0
cvtsi2sd xmm0, rdx
addsd xmm1, xmm0
movsd xmm0, xmm1
ret

function_call_test_start:
sub rsp, 40
movsd xmm0, qword [function_call_test_start_C0]
cvtsi2sd xmm1, rcx
addsd xmm0, xmm1
mov rdx, rcx
sal rdx, 1
mov rcx, rcx
call function_call_test_end
add rsp, 40
ret

function_run:
sub rsp, 56
movsd xmm0, qword [function_run_C0]
movsd xmm1, qword [function_run_C1]
call function_basic_calculation
movsd xmm2, xmm0
movsd xmm1, qword [function_run_C1]
movsd xmm0, qword [function_run_C0]
call function_decimal_array_test
mov rcx, 7
movsd qword [rsp+48], xmm0
call function_call_test_start
movsd xmm1, qword [function_run_C0]
addsd xmm1, qword [function_run_C1]
addsd xmm1, xmm0
addsd xmm1, qword [rsp+48]
subsd xmm1, xmm0
movsd xmm0, xmm1
add rsp, 56
ret

section .data

function_basic_calculation_C0 dq 1
function_basic_calculation_C1 dq 3.14159
function_decimal_array_test_C0 dq 7
function_call_test_start_C0 dq 1
function_run_C0 dq 100
function_run_C1 dq 700
function_run_C1 dq 700
function_run_C0 dq 100
function_run_C0 dq 100
function_run_C1 dq 700