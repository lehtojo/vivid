section .text
global main
main:
jmp _V4initv_rx

extern _V4copyPhxPS_
extern _V11offset_copyPhxPS_x
extern _V14internal_printPhx
extern _V17internal_allocatex_rPh

_V9fibonaccix:
push rsi
push rbx
push rdi
push rbp
push r12
sub rsp, 48
xor rbx, rbx
mov rdi, 1
xor rbp, rbp
mov r12, rcx
cmp rbx, r12
jge _V9fibonaccix_L1
_V9fibonaccix_L0:
cmp rbx, 1
jg _V9fibonaccix_L4
mov rsi, rbx
jmp _V9fibonaccix_L3
_V9fibonaccix_L4:
lea rsi, [rbp+rdi]
mov rbp, rdi
mov rdi, rsi
_V9fibonaccix_L3:
mov rcx, rsi
call _V9to_stringx_rP6String
mov rcx, rax
call _V7printlnP6String
add rbx, 1
cmp rbx, r12
jl _V9fibonaccix_L0
_V9fibonaccix_L1:
add rsp, 48
pop r12
pop rbp
pop rdi
pop rbx
pop rsi
ret

_V4initv_rx:
sub rsp, 40
mov rcx, 10
call _V9fibonaccix
xor rax, rax
add rsp, 40
ret

_V9to_stringx_rP6String:
push rbx
push rsi
push rdi
push rbp
sub rsp, 40
mov rbx, rcx
lea rcx, [rel _V9to_stringx_rP6String_S0]
call _VN6String4initEPh_rPS_
lea rcx, [rel _V9to_stringx_rP6String_S1]
mov rsi, rax
call _VN6String4initEPh_rPS_
mov rdi, rax
test rbx, rbx
jge _V9to_stringx_rP6String_L0
lea rcx, [rel _V9to_stringx_rP6String_S2]
call _VN6String4initEPh_rPS_
mov rdi, rax
mov rcx, rbx
neg rcx
mov rbx, rcx
_V9to_stringx_rP6String_L0:
_V9to_stringx_rP6String_L3:
_V9to_stringx_rP6String_L2:
mov rax, rbx
cqo
mov rcx, 10
idiv rcx
mov rax, rbx
mov rbp, rdx
mov rcx, 1844674407370955162
mul rcx
mov rbx, rdx
sar rbx, 63
add rbx, rdx
mov r8, 48
add r8, rbp
mov rcx, rsi
xor rdx, rdx
call _VN6String6insertExh_rPS_
mov rsi, rax
test rbx, rbx
jne _V9to_stringx_rP6String_L5
mov rcx, rdi
mov rdx, rsi
call _VN6String7combineEPS__rS0_
add rsp, 40
pop rbp
pop rdi
pop rsi
pop rbx
ret
_V9to_stringx_rP6String_L5:
jmp _V9to_stringx_rP6String_L2
_V9to_stringx_rP6String_L4:
add rsp, 40
pop rbp
pop rdi
pop rsi
pop rbx
ret

_V7printlnP6String:
push rbx
sub rsp, 48
mov rdx, 10
mov rbx, rcx
call _VN6String6appendEh_rPS_
mov rcx, rax
call _VN6String4dataEv_rPh
mov rcx, rbx
mov rbx, rax
call _VN6String6lengthEv_rx
add rax, 1
mov rcx, rbx
mov rdx, rax
call _V14internal_printPhx
add rsp, 48
pop rbx
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

_VN6String7combineEPS__rS0_:
push rbx
push rsi
push rdi
push rbp
sub rsp, 40
mov rbx, rcx
mov rsi, rdx
call _VN6String6lengthEv_rx
mov rcx, rsi
mov rdi, rax
call _VN6String6lengthEv_rx
lea rbp, [rax+1]
lea rcx, [rdi+rbp]
call _V8allocatex_rPh
mov rcx, [rbx+8]
mov rdx, rdi
mov r8, rax
mov rbx, rax
call _V4copyPhxPS_
mov rcx, [rsi+8]
mov rdx, rbp
mov r8, rbx
mov r9, rdi
call _V11offset_copyPhxPS_x
mov rcx, rbx
call _VN6String4initEPh_rPS_
add rsp, 40
pop rbp
pop rdi
pop rsi
pop rbx
ret

_VN6String6appendEh_rPS_:
push rbx
push rsi
push rdi
sub rsp, 48
mov rbx, rcx
mov rsi, rdx
call _VN6String6lengthEv_rx
lea rcx, [rax+2]
mov rdi, rax
call _V8allocatex_rPh
mov rcx, [rbx+8]
mov rdx, rdi
mov r8, rax
mov rbx, rax
call _V4copyPhxPS_
mov byte [rbx+rdi], sil
add rdi, 1
mov byte [rbx+rdi], 0
mov rcx, rbx
call _VN6String4initEPh_rPS_
add rsp, 48
pop rdi
pop rsi
pop rbx
ret

_VN6String6insertExh_rPS_:
push rbx
push rsi
push rdi
push rbp
push r12
sub rsp, 48
mov rbx, rcx
mov rsi, rdx
mov rdi, r8
call _VN6String6lengthEv_rx
lea rcx, [rax+2]
mov rbp, rax
call _V8allocatex_rPh
mov rcx, [rbx+8]
mov rdx, rsi
mov r8, rax
mov r12, rax
call _V4copyPhxPS_
mov rcx, [rbx+8]
mov rdx, rbp
sub rdx, rsi
lea r9, [rsi+1]
mov r8, r12
call _V11offset_copyPhxPS_x
mov byte [r12+rsi], dil
add rbp, 1
mov byte [r12+rbp], 0
mov rcx, r12
call _VN6String4initEPh_rPS_
add rsp, 48
pop r12
pop rbp
pop rdi
pop rsi
pop rbx
ret

_VN6String4dataEv_rPh:
mov rax, [rcx+8]
ret

_VN6String6lengthEv_rx:
xor rax, rax
mov rdx, [rcx+8]
movzx r8, byte [rdx+rax]
test r8, r8
je _VN6String6lengthEv_rx_L1
_VN6String6lengthEv_rx_L0:
add rax, 1
mov rdx, [rcx+8]
movzx r8, byte [rdx+rax]
test r8, r8
jne _VN6String6lengthEv_rx_L0
_VN6String6lengthEv_rx_L1:
ret

section .data

_VN10Allocation_current dq 0

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
_V9to_stringx_rP6String_S0 db 0
align 16
_V9to_stringx_rP6String_S1 db 0
align 16
_V9to_stringx_rP6String_S2 db '-', 0