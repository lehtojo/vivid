section .text
global _start
_start:
call function_run
mov rax, 60
xor rdi, rdi
syscall

extern function_allocate
extern function_integer_power
extern function_sys_print
extern function_sys_read
extern function_copy
extern function_offset_copy
extern function_free

global function_simple_math
function_simple_math:
mov rax, rax
ret

function_run:
push 3
push 2
push 1
call function_simple_math
add rsp, 24
ret

section .data