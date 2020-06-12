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

global function_multi_return
export function_multi_return
function_multi_return:
push rbx
push rsi
sub rsp, 40
mov rbx, rcx
mov rsi, rdx
call large_function
cmp rbx, rsi
jle function_multi_return_L1
mov rax, 1
add rsp, 40
pop rsi
pop rbx
ret
jmp function_multi_return_L0
function_multi_return_L1:
cmp rbx, rsi
jge function_multi_return_L3
mov rax, -1
add rsp, 40
pop rsi
pop rbx
ret
jmp function_multi_return_L0
function_multi_return_L3:
xor rax, rax
add rsp, 40
pop rsi
pop rbx
ret
function_multi_return_L0:
add rsp, 40
pop rsi
pop rbx
ret

function_run:
sub rsp, 40
mov rcx, 10
xor rdx, rdx
call function_multi_return
mov rax, 1
add rsp, 40
ret

section .data