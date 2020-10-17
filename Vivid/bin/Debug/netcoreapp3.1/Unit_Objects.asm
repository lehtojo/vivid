section .text
global main
main:
jmp _V4initv_rx

extern _V8allocatex_rPh

global _V12create_applev_rP5Apple
export _V12create_applev_rP5Apple
_V12create_applev_rP5Apple:
sub rsp, 40
call _VN5Apple4initEv_rPh
add rsp, 40
ret

global _V10create_card_rP3Car
export _V10create_card_rP3Car
_V10create_card_rP3Car:
sub rsp, 40
call _VN3Car4initEd_rPh
add rsp, 40
ret

_V4initv_rx:
sub rsp, 40
mov rax, 1
add rsp, 40
ret
call _V12create_applev_rP5Apple
pxor xmm0, xmm0
call _V10create_card_rP3Car
ret

_VN5Apple4initEv_rPh:
sub rsp, 40
mov rcx, 16
call _V8allocatex_rPh
mov qword [rax], 100
movsd xmm0, qword [rel _VN5Apple4initEv_rPh_C0]
movsd qword [rax+8], xmm0
add rsp, 40
ret

_VN3Car4initEd_rPh:
push rbx
sub rsp, 48
mov rcx, 24
movsd qword [rsp+64], xmm0
call _V8allocatex_rPh
mov qword [rax+8], 2000000
lea rcx, [rel _VN3Car4initEd_rPh_S0]
mov rbx, rax
call _VN6String4initEPh_rS0_
mov qword [rbx+16], rax
movsd xmm0, qword [rsp+64]
movsd qword [rbx], xmm0
mov rax, rbx
add rsp, 48
pop rbx
ret

_VN6String4initEPh_rS0_:
push rbx
sub rsp, 48
mov rbx, rcx
mov rcx, 8
call _V8allocatex_rPh
mov qword [rax], rbx
add rsp, 48
pop rbx
ret

section .data

align 16
_VN3Car4initEd_rPh_S0 db 'Flash', 0
align 16
_VN5Apple4initEv_rPh_C0 db 154, 153, 153, 153, 153, 153, 185, 63 ; 0.1