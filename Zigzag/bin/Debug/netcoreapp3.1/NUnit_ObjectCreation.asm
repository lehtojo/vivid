section .text
global _start
_start:
call _V4initv_rx
mov rax, 60
xor rdi, rdi
syscall

extern _V8allocatex_rPh

global _V12create_applev_rP5Apple
_V12create_applev_rP5Apple:
sub rsp, 8
call _VN5Apple4initEv_rPh
add rsp, 8
ret

global _V10create_card_rP3Car
_V10create_card_rP3Car:
sub rsp, 8
movsd xmm1, xmm0
cvtsd2si rcx, xmm1
cvtsi2sd xmm0, rcx
call _VN3Car4initEd_rPh
add rsp, 8
ret

_V4initv_rx:
sub rsp, 8
mov rax, 1
add rsp, 8
ret
call _V12create_applev_rP5Apple
movsd xmm0, qword [rel _V4initv_rx_C0]
call _V10create_card_rP3Car
ret

_VN5Apple4initEv_rPh:
sub rsp, 8
mov rdi, 16
call _V8allocatex_rPh
mov qword [rax], 100
movsd xmm0, qword [rel _VN5Apple4initEv_rPh_C0]
movsd qword [rax+8], xmm0
add rsp, 8
ret

_VN3Car4initEd_rPh:
push rbx
sub rsp, 16
mov rdi, 24
movsd qword [rsp+8], xmm0
call _V8allocatex_rPh
mov qword [rax+8], 2000000
lea rdi, [rel _VN3Car4initEd_rPh_S0]
mov rbx, rax
call _VN6String4initEPh_rS0_
mov qword [rbx+16], rax
movsd xmm0, qword [rsp+8]
movsd qword [rbx], xmm0
mov rax, rbx
add rsp, 16
pop rbx
ret

_VN6String4initEPh_rS0_:
push rbx
sub rsp, 16
mov rcx, rdi
mov rdi, 8
mov rbx, rcx
call _V8allocatex_rPh
mov qword [rax], rbx
add rsp, 16
pop rbx
ret

section .data

_VN3Car4initEd_rPh_S0 db 'Flash', 0
_V4initv_rx_C0 dq 0.0
_VN5Apple4initEv_rPh_C0 dq 0.1