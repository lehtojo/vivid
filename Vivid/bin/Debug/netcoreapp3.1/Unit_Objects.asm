section .text
global main
main:
jmp _V4initv_rx

extern _V17internal_allocatex_rPh

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

_V8allocatex_rPh:
push rbx
push rsi
sub rsp, 40
mov r8, [rel _VN10Allocation_current]
test r8, r8
je _V8allocatex_rPh_L0
mov rdx, [r8+16]
lea r9, [rdx+rcx]
cmp r9, 1000000
jg _V8allocatex_rPh_L0
lea r9, [rdx+rcx]
mov qword [r8+16], r9
lea r9, [rdx+rcx]
mov rax, [r8+8]
add rax, rdx
add rsp, 40
pop rsi
pop rbx
ret
_V8allocatex_rPh_L0:
mov rbx, rcx
mov rcx, 1000000
call _V17internal_allocatex_rPh
mov rcx, 24
mov rsi, rax
call _V17internal_allocatex_rPh
mov qword [rax+8], rsi
mov qword [rax+16], rbx
mov qword [rel _VN10Allocation_current], rax
mov rax, rsi
add rsp, 40
pop rsi
pop rbx
ret

_V8inheritsPhPS__rx:
push rbx
push rsi
sub rsp, 16
mov r8, [rcx]
mov r9, [rdx]
movzx r10, byte [r9]
xor rax, rax
_V8inheritsPhPS__rx_L1:
_V8inheritsPhPS__rx_L0:
movzx rcx, byte [r8+rax]
add rax, 1
cmp rcx, r10
jnz _V8inheritsPhPS__rx_L4
mov r11, rcx
mov rbx, 1
_V8inheritsPhPS__rx_L7:
_V8inheritsPhPS__rx_L6:
movzx r11, byte [r8+rax]
movzx rsi, byte [r9+rbx]
add rax, 1
add rbx, 1
cmp r11, rsi
jz _V8inheritsPhPS__rx_L9
cmp r11, 1
jne _V8inheritsPhPS__rx_L9
test rsi, rsi
jne _V8inheritsPhPS__rx_L9
mov rax, 1
add rsp, 16
pop rsi
pop rbx
ret
_V8inheritsPhPS__rx_L9:
jmp _V8inheritsPhPS__rx_L6
_V8inheritsPhPS__rx_L8:
jmp _V8inheritsPhPS__rx_L3
_V8inheritsPhPS__rx_L4:
cmp rcx, 2
jne _V8inheritsPhPS__rx_L3
xor rax, rax
add rsp, 16
pop rsi
pop rbx
ret
_V8inheritsPhPS__rx_L3:
jmp _V8inheritsPhPS__rx_L0
_V8inheritsPhPS__rx_L2:
add rsp, 16
pop rsi
pop rbx
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

_VN10Allocation_current dq 0

_VN5Apple_configuration:
dq _VN5Apple_descriptor

_VN5Apple_descriptor:
dq _VN5Apple_descriptor_0
dd 24
dd 0

_VN5Apple_descriptor_0:
db 'Apple', 0, 1, 2, 0

_VN3Car_configuration:
dq _VN3Car_descriptor

_VN3Car_descriptor:
dq _VN3Car_descriptor_0
dd 32
dd 0

_VN3Car_descriptor_0:
db 'Car', 0, 1, 2, 0

_VN6String_configuration:
dq _VN6String_descriptor

_VN6String_descriptor:
dq _VN6String_descriptor_0
dd 16
dd 0

_VN6String_descriptor_0:
db 'String', 0, 1, 2, 0

_VN4Page_configuration:
dq _VN4Page_descriptor

_VN4Page_descriptor:
dq _VN4Page_descriptor_0
dd 24
dd 0

_VN4Page_descriptor_0:
db 'Page', 0, 1, 2, 0

_VN10Allocation_configuration:
dq _VN10Allocation_descriptor

_VN10Allocation_descriptor:
dq _VN10Allocation_descriptor_0
dd 8
dd 0

_VN10Allocation_descriptor_0:
db 'Allocation', 0, 1, 2, 0

align 16
_VN3Car4initEd_rPS__S0 db 'Flash', 0
align 16
_VN5Apple4initEv_rPS__C0 db 154, 153, 153, 153, 153, 153, 185, 63 ; 0.1