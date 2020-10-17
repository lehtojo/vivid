section .text
global main
main:
jmp _V4initv_rx

extern _V8allocatex_rPh
extern _V14large_functionv

global _V10evacuationxx_rx
export _V10evacuationxx_rx
_V10evacuationxx_rx:
push rbx
push rsi
sub rsp, 40
mov rbx, rcx
mov rsi, rdx
call _V14large_functionv
lea rax, [rbx+rsi]
imul rbx, rsi
add rax, rbx
add rax, 10
add rsp, 40
pop rsi
pop rbx
ret

global _V22evacuation_with_memoryxx_rx
export _V22evacuation_with_memoryxx_rx
_V22evacuation_with_memoryxx_rx:
push rbx
push rsi
sub rsp, 40
mov rbx, rcx
mov rsi, rdx
call _V14large_functionv
lea rax, [rbx+rsi]
imul rbx, rsi
lea rcx, [rbx*2+rbx]
add rax, rcx
add rax, 30
add rsp, 40
pop rsi
pop rbx
ret

_V4initv_rx:
sub rsp, 40
mov rax, 1
add rsp, 40
ret
mov rcx, 1
mov rdx, 1
call _V10evacuationxx_rx
mov rcx, 1
mov rdx, 1
call _V22evacuation_with_memoryxx_rx
ret