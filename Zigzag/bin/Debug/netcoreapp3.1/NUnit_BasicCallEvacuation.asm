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

function_run:
sub rsp, 40
mov rax, 1
add rsp, 40
ret
mov rcx, 1
mov rdx, 1
call function_basic_call_evacuation
ret

section .data