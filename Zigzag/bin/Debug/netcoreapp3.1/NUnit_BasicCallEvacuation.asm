section .text
global _start
_start:
call _V4initv_rx
mov rax, 60
xor rdi, rdi
syscall

extern _V8allocatex_rPh
extern _V14large_functionv

global _V21basic_call_evacuationxx_rx
_V21basic_call_evacuationxx_rx:
push rbx
push rbp
push r12
sub rsp, 16
mov rbx, rdi
imul rbx, rsi
add rbx, 10
mov rbp, rsi
mov r12, rdi
call _V14large_functionv
add r12, rbp
add r12, rbx
mov rax, r12
add rsp, 16
pop r12
pop rbp
pop rbx
ret

global _V33basic_call_evacuation_with_memoryxx_rx
_V33basic_call_evacuation_with_memoryxx_rx:
push rbx
push rbp
push r12
push r13
push r14
sub rsp, 16
mov rbx, rdi
imul rbx, rsi
add rbx, 10
mov rbp, rdi
imul rbp, rsi
add rbp, 10
mov r12, rdi
imul r12, rsi
add r12, 10
mov r13, rsi
mov r14, rdi
call _V14large_functionv
add r14, r13
add r14, rbx
add r14, rbp
add r14, r12
mov rax, r14
add rsp, 16
pop r14
pop r13
pop r12
pop rbp
pop rbx
ret

_V4initv_rx:
sub rsp, 8
mov rax, 1
add rsp, 8
ret
mov rdi, 1
mov rsi, 1
call _V21basic_call_evacuationxx_rx
mov rdi, 1
mov rsi, 1
call _V33basic_call_evacuation_with_memoryxx_rx
ret