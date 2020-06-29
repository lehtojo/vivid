section .text
global function_run
extern allocate
extern integer_power
extern sys_print
extern sys_read
extern copy
extern offset_copy
extern deallocate

global function_single_boolean
export function_single_boolean
function_single_boolean:
cmp rcx, 1
jne function_single_boolean_L1
xor rax, rax
ret
jmp function_single_boolean_L0
function_single_boolean_L1:
mov rax, 1
ret
function_single_boolean_L0:
ret

global function_two_booleans
export function_two_booleans
function_two_booleans:
cmp rcx, 1
jne function_two_booleans_L1
mov rax, 1
ret
jmp function_two_booleans_L0
function_two_booleans_L1:
cmp rdx, 1
jne function_two_booleans_L3
mov rax, 2
ret
jmp function_two_booleans_L0
function_two_booleans_L3:
mov rax, 3
ret
function_two_booleans_L0:
ret

global function_nested_if_statements
export function_nested_if_statements
function_nested_if_statements:
cmp rcx, 1
jne function_nested_if_statements_L1
cmp rdx, 2
jne function_nested_if_statements_L4
cmp r8, 3
jne function_nested_if_statements_L7
mov rax, 1
ret
jmp function_nested_if_statements_L6
function_nested_if_statements_L7:
cmp r8, 4
jne function_nested_if_statements_L6
mov rax, 1
ret
function_nested_if_statements_L6:
jmp function_nested_if_statements_L3
function_nested_if_statements_L4:
test rdx, rdx
jne function_nested_if_statements_L3
cmp r8, 1
jne function_nested_if_statements_L12
mov rax, 1
ret
jmp function_nested_if_statements_L11
function_nested_if_statements_L12:
cmp r8, -1
jne function_nested_if_statements_L11
mov rax, 1
ret
function_nested_if_statements_L11:
function_nested_if_statements_L3:
xor rax, rax
ret
jmp function_nested_if_statements_L0
function_nested_if_statements_L1:
cmp rcx, 2
jne function_nested_if_statements_L0
cmp rdx, 4
jne function_nested_if_statements_L17
cmp r8, 8
jne function_nested_if_statements_L20
mov rax, 1
ret
jmp function_nested_if_statements_L19
function_nested_if_statements_L20:
cmp r8, 6
jne function_nested_if_statements_L19
mov rax, 1
ret
function_nested_if_statements_L19:
jmp function_nested_if_statements_L16
function_nested_if_statements_L17:
cmp rdx, 3
jne function_nested_if_statements_L16
cmp r8, 4
jne function_nested_if_statements_L25
mov rax, 1
ret
jmp function_nested_if_statements_L24
function_nested_if_statements_L25:
cmp r8, 5
jne function_nested_if_statements_L24
mov rax, 1
ret
function_nested_if_statements_L24:
function_nested_if_statements_L16:
xor rax, rax
ret
function_nested_if_statements_L0:
xor rax, rax
ret

global function_logical_and_in_if_statement
export function_logical_and_in_if_statement
function_logical_and_in_if_statement:
cmp rcx, 1
jne function_logical_and_in_if_statement_L0
cmp rdx, 1
jne function_logical_and_in_if_statement_L0
mov rax, 10
ret
function_logical_and_in_if_statement_L0:
xor rax, rax
ret

global function_logical_or_in_if_statement
export function_logical_or_in_if_statement
function_logical_or_in_if_statement:
cmp rcx, 1
je function_logical_or_in_if_statement_L1
cmp rdx, 1
jne function_logical_or_in_if_statement_L0
function_logical_or_in_if_statement_L1:
mov rax, 10
ret
function_logical_or_in_if_statement_L0:
xor rax, rax
ret

global function_nested_logical_statements
export function_nested_logical_statements
function_nested_logical_statements:
cmp rcx, 1
jne function_nested_logical_statements_L1
cmp rdx, 1
jne function_nested_logical_statements_L1
cmp r8, 1
jne function_nested_logical_statements_L1
cmp r9, 1
jne function_nested_logical_statements_L1
mov rax, 1
ret
jmp function_nested_logical_statements_L0
function_nested_logical_statements_L1:
cmp rcx, 1
je function_nested_logical_statements_L8
cmp rdx, 1
jne function_nested_logical_statements_L6
function_nested_logical_statements_L8:
cmp r8, 1
jne function_nested_logical_statements_L6
cmp r9, 1
jne function_nested_logical_statements_L6
mov rax, 2
ret
jmp function_nested_logical_statements_L0
function_nested_logical_statements_L6:
cmp rcx, 1
jne function_nested_logical_statements_L11
cmp rdx, 1
jne function_nested_logical_statements_L11
cmp r8, 1
je function_nested_logical_statements_L12
cmp r9, 1
jne function_nested_logical_statements_L11
function_nested_logical_statements_L12:
mov rax, 3
ret
jmp function_nested_logical_statements_L0
function_nested_logical_statements_L11:
cmp rcx, 1
jne function_nested_logical_statements_L18
cmp rdx, 1
je function_nested_logical_statements_L17
function_nested_logical_statements_L18:
cmp r8, 1
jne function_nested_logical_statements_L16
cmp r9, 1
jne function_nested_logical_statements_L16
function_nested_logical_statements_L17:
mov rax, 4
ret
jmp function_nested_logical_statements_L0
function_nested_logical_statements_L16:
cmp rcx, 1
je function_nested_logical_statements_L22
cmp rdx, 1
je function_nested_logical_statements_L22
cmp r8, 1
je function_nested_logical_statements_L22
cmp r9, 1
jne function_nested_logical_statements_L21
function_nested_logical_statements_L22:
mov rax, 5
ret
jmp function_nested_logical_statements_L0
function_nested_logical_statements_L21:
mov rax, 6
ret
function_nested_logical_statements_L0:
ret

global function_logical_operators_1
export function_logical_operators_1
function_logical_operators_1:
cmp rcx, rdx
jg function_logical_operators_1_L2
test rcx, rcx
jne function_logical_operators_1_L1
function_logical_operators_1_L2:
mov rax, rdx
ret
jmp function_logical_operators_1_L0
function_logical_operators_1_L1:
cmp rcx, rdx
jne function_logical_operators_1_L4
cmp rdx, 1
jne function_logical_operators_1_L4
mov rax, rcx
ret
jmp function_logical_operators_1_L0
function_logical_operators_1_L4:
xor rax, rax
ret
function_logical_operators_1_L0:
ret

global function_logical_operators_2
export function_logical_operators_2
function_logical_operators_2:
cmp rcx, rdx
jle function_logical_operators_2_L3
cmp rcx, r8
jg function_logical_operators_2_L2
function_logical_operators_2_L3:
cmp r8, rdx
jle function_logical_operators_2_L1
function_logical_operators_2_L2:
mov rax, 1
ret
jmp function_logical_operators_2_L0
function_logical_operators_2_L1:
cmp rcx, rdx
jle function_logical_operators_2_L7
cmp rdx, r8
jl function_logical_operators_2_L5
function_logical_operators_2_L7:
cmp r8, 1
je function_logical_operators_2_L6
cmp rcx, 1
jne function_logical_operators_2_L5
function_logical_operators_2_L6:
xor rax, rax
ret
jmp function_logical_operators_2_L0
function_logical_operators_2_L5:
mov rax, -1
ret
function_logical_operators_2_L0:
ret

function_f:
cmp rcx, 7
jne function_f_L1
mov rax, 1
ret
jmp function_f_L0
function_f_L1:
xor rax, rax
ret
function_f_L0:
ret

global function_logical_operators_3
export function_logical_operators_3
function_logical_operators_3:
push rbx
push rsi
sub rsp, 40
cmp rcx, 10
jg function_logical_operators_3_L3
mov rbx, rcx
mov rsi, rdx
call function_f
cmp rax, 1
mov rcx, rbx
mov rdx, rsi
jne function_logical_operators_3_L1
function_logical_operators_3_L3:
cmp rcx, rdx
jle function_logical_operators_3_L1
xor rax, rax
add rsp, 40
pop rsi
pop rbx
ret
jmp function_logical_operators_3_L0
function_logical_operators_3_L1:
mov rax, 1
add rsp, 40
pop rsi
pop rbx
ret
function_logical_operators_3_L0:
add rsp, 40
pop rsi
pop rbx
ret

function_run:
sub rsp, 40
mov rcx, 1
mov rdx, 1
call function_logical_operators_1
mov rcx, 1
mov rdx, 1
mov r8, 1
call function_logical_operators_2
mov rcx, 1
mov rdx, 1
call function_logical_operators_3
mov rcx, 1
call function_single_boolean
mov rcx, 1
mov rdx, 1
call function_two_booleans
xor rcx, rcx
xor rdx, rdx
xor r8, r8
call function_nested_if_statements
mov rcx, 1
mov rdx, 1
call function_logical_and_in_if_statement
mov rcx, 1
mov rdx, 1
call function_logical_or_in_if_statement
mov rcx, 1
mov rdx, 1
mov r8, 1
mov r9, 1
call function_nested_logical_statements
mov rax, 1
add rsp, 40
ret

section .data