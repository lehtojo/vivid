section .text
global main
main:
jmp _V4initv_rx

extern _V8allocatex_rPh

_V1fxxxxxx_rx:
add rcx, rdx
add rcx, r8
add rcx, r9
add rcx, [rsp+40]
add rcx, [rsp+48]
mov rax, rcx
ret

global _V1gxx_rx
export _V1gxx_rx
_V1gxx_rx:
push rbx
sub rsp, 48
lea r8, [rcx+1]
mov r9, rcx
sar r9, 1
sal rcx, 2
lea r10, [rdx+1]
mov r11, rdx
sal r11, 1
sar rdx, 2
mov qword [rsp+40], rdx
mov rbx, r9
mov r9, r10
mov qword [rsp+32], r11
mov rdx, rbx
xchg r8, rcx
call _V1fxxxxxx_rx
add rsp, 48
pop rbx
ret

_V4initv_rx:
sub rsp, 40
mov rcx, 1
mov rdx, 1
call _V1gxx_rx
mov rax, 1
add rsp, 40
ret