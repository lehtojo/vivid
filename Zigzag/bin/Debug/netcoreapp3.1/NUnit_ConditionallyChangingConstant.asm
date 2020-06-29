section .text
global function_run
extern allocate
extern integer_power
extern sys_print
extern sys_read
extern copy
extern offset_copy
extern deallocate

global function_conditionally_changing_constant_with_if_statement
export function_conditionally_changing_constant_with_if_statement
function_conditionally_changing_constant_with_if_statement:
mov r8, 7
cmp rcx, rdx
jle function_conditionally_changing_constant_with_if_statement_L0
mov r8, rcx
function_conditionally_changing_constant_with_if_statement_L0:
add rcx, r8
mov rax, rcx
ret

global function_conditionally_changing_constant_with_loop_statement
export function_conditionally_changing_constant_with_loop_statement
function_conditionally_changing_constant_with_loop_statement:
mov rax, 100
cmp rcx, rdx
jge function_conditionally_changing_constant_with_loop_statement_L1
function_conditionally_changing_constant_with_loop_statement_L0:
add rax, 1
add rcx, 1
cmp rcx, rdx
jl function_conditionally_changing_constant_with_loop_statement_L0
function_conditionally_changing_constant_with_loop_statement_L1:
imul rdx, rax
mov rax, rdx
ret

function_run:
sub rsp, 40
mov rax, 1
add rsp, 40
ret
mov rcx, 1
mov rdx, 1
call function_conditionally_changing_constant_with_if_statement
mov rcx, 1
mov rdx, 1
call function_conditionally_changing_constant_with_loop_statement
ret

section .data