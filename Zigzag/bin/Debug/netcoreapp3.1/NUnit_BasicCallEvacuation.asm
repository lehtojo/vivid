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
mov rbx, rcx
imul rbx, rdx
add rbx, 10
mov rsi, rcx
mov rdi, rdx
call large_function
add rsi, rdi
add rsi, rbx
mov rax, rsi
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
mov rbx, rcx
imul rbx, rdx
add rbx, 10
mov rsi, rcx
imul rsi, rdx
add rsi, 10
mov rdi, rcx
imul rdi, rdx
add rdi, 10
mov rbp, rcx
mov r12, rdx
call large_function
add rbp, r12
add rbp, rbx
add rbp, rsi
add rbp, rdi
mov rax, rbp
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