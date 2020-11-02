section .text
global main
main:
jmp _V4initv_rx

extern _V8allocatex_rPh

global _V12create_applev_rP5Apple
export _V12create_applev_rP5Apple
_V12create_applev_rP5Apple:
sub rsp, 40
call _VN5Apple4initEv_rPS_
add rsp, 40
ret

global _V10create_card_rP3Car
export _V10create_card_rP3Car
_V10create_card_rP3Car:
sub rsp, 40
call _VN3Car4initEd_rPS_
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

_VN5Apple4initEv_rPS_:
sub rsp, 40
mov rcx, 24
call _V8allocatex_rPh
movsd xmm0, qword [rel _VN5Apple4initEv_rPS__C0]
movsd qword [rax+16], xmm0
mov qword [rax+8], 100
add rsp, 40
ret

_VN3Car4initEd_rPS_:
push rbx
sub rsp, 48
mov rcx, 32
movsd qword [rsp+64], xmm0
call _V8allocatex_rPh
lea rcx, [rel _VN3Car4initEd_rPS__S0]
mov rbx, rax
call _VN6String4initEPh_rPS_
mov qword [rbx+24], rax
mov qword [rbx+16], 2000000
movsd xmm0, qword [rsp+64]
movsd qword [rbx+8], xmm0
mov rax, rbx
add rsp, 48
pop rbx
ret

_VN6String4initEPh_rPS_:
push rbx
sub rsp, 48
mov rbx, rcx
mov rcx, 16
call _V8allocatex_rPh
mov qword [rax+8], rbx
add rsp, 48
pop rbx
ret

section .data

_VN5Apple_configuration:
dq _VN5Apple_descriptor

_VN5Apple_descriptor:
dq _VN5Apple_descriptor_0
dd 24
dd 0

_VN5Apple_descriptor_0:
db 'Apple', 0

_VN3Car_configuration:
dq _VN3Car_descriptor

_VN3Car_descriptor:
dq _VN3Car_descriptor_0
dd 32
dd 0

_VN3Car_descriptor_0:
db 'Car', 0

_VN6String_configuration:
dq _VN6String_descriptor

_VN6String_descriptor:
dq _VN6String_descriptor_0
dd 16
dd 0

_VN6String_descriptor_0:
db 'String', 0

align 16
_VN3Car4initEd_rPS__S0 db 'Flash', 0
align 16
_VN5Apple4initEv_rPS__C0 db 154, 153, 153, 153, 153, 153, 185, 63 ; 0.1