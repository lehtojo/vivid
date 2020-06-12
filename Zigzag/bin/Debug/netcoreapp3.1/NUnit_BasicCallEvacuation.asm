section .text
global function_run
extern allocate
extern integer_power
extern sys_print
extern sys_read
extern copy
extern offset_copy
extern deallocate
extern large_function

global function_basic_call_evacuation
export function_basic_call_evacuation
function_basic_call_evacuation:
push rbx
push rsi
push rdi
sub rsp, 48
mov r8, rcx
imul r8, rdx
add r8, 10
mov rbx, rcx
mov rsi, rdx
mov rdi, r8
call large_function
add rbx, rsi
add rbx, rdi
mov rax, rbx
add rsp, 48
pop rdi
pop rsi
pop rbx
ret

global function_basic_call_evacuation_with_memory
export function_basic_call_evacuation_with_memory
function_basic_call_evacuation_with_memory:
push rbx
push rsi
push rdi
push rbp
push r12
sub rsp, 48
mov r8, rcx
imul r8, rdx
add r8, 10
mov r9, rcx
imul r9, rdx
add r9, 10
mov r10, rcx
imul r10, rdx
add r10, 10
mov rbx, rcx
mov rsi, rdx
mov rdi, r8
mov rbp, r9
mov r12, r10
call large_function
add rbx, rsi
add rbx, rdi
add rbx, rbp
add rbx, r12
mov rax, rbx
add rsp, 48
pop r12
pop rbp
pop rdi
pop rsi
pop rbx
ret

function_run:
sub rsp, 40
mov rax, 1
add rsp, 40
ret
mov rcx, 1
mov rdx, 1
call function_basic_call_evacuation
mov rcx, 1
mov rdx, 1
call function_basic_call_evacuation_with_memory
ret

section .data