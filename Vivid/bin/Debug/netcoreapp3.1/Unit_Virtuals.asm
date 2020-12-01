section .text
global main
main:
jmp _V4initv_rx

extern _V4copyPhxPS_
extern _V11offset_copyPhxPS_x
extern _V14internal_printPhx
extern _V17internal_allocatex_rPh

_V24execute_virtual_type_onev:
push rbx
sub rsp, 48
call _VN14VirtualTypeOne4initEv_rPS_
mov rcx, rax
mov rbx, rax
call _VN14VirtualTypeOne3fooEv
mov rcx, [rbx]
mov rdx, [rcx+8]
mov rcx, rbx
call rdx
add rsp, 48
pop rbx
ret

_V24execute_virtual_type_twov:
push rbx
sub rsp, 48
call _VN14VirtualTypeTwo4initEv_rPS_
mov qword [rax+8], 7
mov rcx, 4631107791820423168
mov qword [rax+24], rcx
mov rcx, rax
mov rbx, rax
call _VN14VirtualTypeTwo3barEv
mov rcx, [rbx]
mov rdx, [rcx+8]
mov rcx, rbx
call rdx
add rsp, 48
pop rbx
ret

_V26execute_virtual_type_threev:
push rbx
sub rsp, 48
call _VN16VirtualTypeThree4initEv_rPS_
mov qword [rax+8], 1
mov rcx, 4621819117588971520
mov qword [rax+24], rcx
mov rcx, rax
mov rdx, 1
mov r8, -1
mov rbx, rax
call _VN16VirtualTypeThree3bazEcs_rx
mov rcx, rax
call _V7printlnx
mov rcx, [rbx]
mov r9, [rcx+8]
mov rcx, rbx
mov rdx, 255
mov r8, 32767
call r9
mov rcx, rax
call _V7printlnx
mov rcx, [rbx]
mov r9, [rcx+8]
mov rcx, rbx
mov rdx, 7
mov r8, 7
call r9
mov rcx, rax
call _V7printlnx
add rsp, 48
pop rbx
ret

_V25execute_virtual_type_fourv:
push rbx
sub rsp, 48
call _VN15VirtualTypeFour4initEv_rPS_
mov qword [rax+16], -6942
mov qword [rax+32], 4269
mov rcx, rax
mov rbx, rax
call _VN15VirtualTypeFour3fooEv
mov rcx, rbx
call _VN15VirtualTypeFour3barEv
mov rcx, rbx
mov rdx, 64
mov r8, 8
call _VN15VirtualTypeFour3bazEcs_rx
mov rcx, rax
call _V7printlnx
mov rcx, [rbx]
mov rdx, [rcx+8]
mov rcx, rbx
call rdx
lea rdx, [rbx+8]
mov rcx, [rdx]
mov r8, [rcx+8]
lea rcx, [rbx+8]
call r8
lea rdx, [rbx+24]
mov rcx, [rdx]
mov r9, [rcx+8]
lea rcx, [rbx+24]
xor rdx, rdx
mov r8, 1
call r9
mov rcx, rax
call _V7printlnx
add rsp, 48
pop rbx
ret

_V4initv_rx:
sub rsp, 40
call _V24execute_virtual_type_onev
call _V24execute_virtual_type_twov
call _V26execute_virtual_type_threev
call _V25execute_virtual_type_fourv
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

_V9to_stringd_rP6String:
push rbx
push rsi
sub rsp, 40
cvttsd2si rcx, xmm0
movsd qword [rsp+64], xmm0
call _V9to_stringx_rP6String
movsd xmm0, qword [rsp+64]
pxor xmm1, xmm1
comisd xmm0, xmm1
jae _V9to_stringd_rP6String_L0
movsd xmm1, xmm0
xorpd xmm1, oword [rel _V9to_stringd_rP6String_C0]
movsd xmm0, xmm1
_V9to_stringd_rP6String_L0:
cvttsd2si rcx, xmm0
cvtsi2sd xmm1, rcx
subsd xmm0, xmm1
mov rbx, rax
pxor xmm1, xmm1
comisd xmm0, xmm1
jnz _V9to_stringd_rP6String_L2
lea rcx, [rel _V9to_stringd_rP6String_S0]
movsd qword [rsp+64], xmm0
call _VN6String4initEPh_rPS_
mov rcx, rbx
mov rdx, rax
call _VN6String7combineEPS__rS0_
add rsp, 40
pop rsi
pop rbx
ret
movsd xmm0, qword [rsp+8]
_V9to_stringd_rP6String_L2:
mov rcx, rbx
mov rdx, 44
movsd qword [rsp+64], xmm0
call _VN6String6appendEh_rPS_
mov rbx, rax
xor rsi, rsi
cmp rsi, 15
jge _V9to_stringd_rP6String_L5
movsd xmm0, qword [rsp+64]
pxor xmm1, xmm1
comisd xmm0, xmm1
movsd qword [rsp+64], xmm0
jbe _V9to_stringd_rP6String_L5
_V9to_stringd_rP6String_L4:
movsd xmm0, qword [rsp+64]
movsd xmm1, qword [rel _V9to_stringd_rP6String_C1]
mulsd xmm0, xmm1
cvttsd2si rcx, xmm0
mov rdx, rcx
cvtsi2sd xmm1, rdx
subsd xmm0, xmm1
movsd xmm2, qword [rel _V9to_stringd_rP6String_C2]
addsd xmm2, xmm1
mov rcx, rbx
cvttsd2si rdx, xmm2
movsd qword [rsp+64], xmm0
call _VN6String6appendEh_rPS_
mov rbx, rax
add rsi, 1
cmp rsi, 15
jge _V9to_stringd_rP6String_L8
movsd xmm0, qword [rsp+64]
pxor xmm1, xmm1
comisd xmm0, xmm1
movsd qword [rsp+64], xmm0
ja _V9to_stringd_rP6String_L4
_V9to_stringd_rP6String_L8:
_V9to_stringd_rP6String_L5:
mov rax, rbx
add rsp, 40
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

_V7printlnc:
sub rsp, 40
call _V9to_stringx_rP6String
mov rcx, rax
call _V7printlnP6String
add rsp, 40
ret

_V7printlns:
sub rsp, 40
call _V9to_stringx_rP6String
mov rcx, rax
call _V7printlnP6String
add rsp, 40
ret

_V7printlnx:
sub rsp, 40
call _V9to_stringx_rP6String
mov rcx, rax
call _V7printlnP6String
add rsp, 40
ret

_V7printlnd:
sub rsp, 40
call _V9to_stringd_rP6String
mov rcx, rax
call _V7printlnP6String
add rsp, 40
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

_VN14VirtualTypeOne4initEv_rPS_:
sub rsp, 40
mov rcx, 16
call _V8allocatex_rPh
lea rcx, [rel _VN14VirtualTypeOne_configuration+8]
mov qword [rax], rcx
add rsp, 40
ret

_VN14VirtualTypeOne3fooEv_v:
_VN14VirtualTypeOne3fooEv:
sub rsp, 40
mov rcx, 3
call _V7printlnx
add rsp, 40
ret

_VN14VirtualTypeTwo4initEv_rPS_:
sub rsp, 40
mov rcx, 32
call _V8allocatex_rPh
lea rcx, [rel _VN14VirtualTypeTwo_configuration+8]
mov qword [rax], rcx
add rsp, 40
ret

_VN14VirtualTypeTwo3barEv_v:
_VN14VirtualTypeTwo3barEv:
sub rsp, 40
mov rdx, [rcx+8]
imul rdx, rdx
movsd xmm0, qword [rcx+24]
mulsd xmm0, xmm0
cvtsi2sd xmm1, rdx
addsd xmm1, xmm0
movsd xmm0, xmm1
call _V7printlnd
add rsp, 40
ret

_VN16VirtualTypeThree4initEv_rPS_:
sub rsp, 40
mov rcx, 32
call _V8allocatex_rPh
lea rcx, [rel _VN16VirtualTypeThree_configuration+8]
mov qword [rax], rcx
add rsp, 40
ret

_VN16VirtualTypeThree3bazEcs_rx_v:
_VN16VirtualTypeThree3bazEcs_rx:
push rbx
push rsi
push rdi
sub rsp, 48
mov rbx, rdx
mov rsi, r8
mov rdi, rcx
cmp rbx, rsi
jle _VN16VirtualTypeThree3bazEcs_rx_L1
mov rcx, rbx
call _V7printlnc
mov rax, rbx
add rsp, 48
pop rdi
pop rsi
pop rbx
ret
mov rbx, rax
jmp _VN16VirtualTypeThree3bazEcs_rx_L0
_VN16VirtualTypeThree3bazEcs_rx_L1:
cmp rsi, rbx
jle _VN16VirtualTypeThree3bazEcs_rx_L3
mov rcx, rsi
call _V7printlns
mov rax, rsi
add rsp, 48
pop rdi
pop rsi
pop rbx
ret
mov rsi, rax
jmp _VN16VirtualTypeThree3bazEcs_rx_L0
_VN16VirtualTypeThree3bazEcs_rx_L3:
movsd xmm0, qword [rdi+24]
call _V7printlnd
movsd xmm0, qword [rdi+24]
cvttsd2si rax, xmm0
add rsp, 48
pop rdi
pop rsi
pop rbx
ret
_VN16VirtualTypeThree3bazEcs_rx_L0:
add rsp, 48
pop rdi
pop rsi
pop rbx
ret

_VN15VirtualTypeFour4initEv_rPS_:
sub rsp, 40
mov rcx, 48
call _V8allocatex_rPh
lea rcx, [rel _VN15VirtualTypeFour_configuration+40]
mov qword [rax+24], rcx
lea rcx, [rel _VN15VirtualTypeFour_configuration+24]
mov qword [rax+8], rcx
lea rcx, [rel _VN15VirtualTypeFour_configuration+8]
mov qword [rax], rcx
add rsp, 40
ret

_VN15VirtualTypeFour3fooEv_v:
_VN15VirtualTypeFour3fooEv:
add qword [rcx+16], 1
sub qword [rcx+32], 1
ret

_VN15VirtualTypeFour3barEv_v:
sub rcx, 8
_VN15VirtualTypeFour3barEv:
mov rdx, [rcx+16]
imul rdx, 7
mov qword [rcx+16], rdx
mov rdx, [rcx+32]
imul rdx, 7
mov qword [rcx+32], rdx
ret

_VN15VirtualTypeFour3bazEcs_rx_v:
sub rcx, 24
_VN15VirtualTypeFour3bazEcs_rx:
mov rax, [rcx+16]
mov r9, [rcx+32]
mov r10, rdx
cqo
idiv r9
mov rcx, rax
mov rax, r10
cqo
idiv r8
add rcx, rax
mov rax, rcx
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

_VN13InheritantOne_configuration:
dq _VN13InheritantOne_descriptor

_VN13InheritantOne_descriptor:
dq _VN13InheritantOne_descriptor_0
dd 8
dd 0

_VN13InheritantOne_descriptor_0:
db 'InheritantOne', 0, 1, 2, 0

_VN14VirtualTypeOne_configuration:
dq _VN14VirtualTypeOne_descriptor
dq _VN14VirtualTypeOne_descriptor
dq _VN14VirtualTypeOne3fooEv_v

_VN14VirtualTypeOne_descriptor:
dq _VN14VirtualTypeOne_descriptor_0
dd 16
dd 1
dq _VN13InheritantOne_descriptor

_VN14VirtualTypeOne_descriptor_0:
db 'VirtualTypeOne', 0, 1, 'InheritantOne', 1, 2, 0

_VN13InheritantTwo_configuration:
dq _VN13InheritantTwo_descriptor

_VN13InheritantTwo_descriptor:
dq _VN13InheritantTwo_descriptor_0
dd 16
dd 0

_VN13InheritantTwo_descriptor_0:
db 'InheritantTwo', 0, 1, 2, 0

_VN14VirtualTypeTwo_configuration:
dq _VN14VirtualTypeTwo_descriptor
dq _VN14VirtualTypeTwo_descriptor
dq _VN14VirtualTypeTwo3barEv_v

_VN14VirtualTypeTwo_descriptor:
dq _VN14VirtualTypeTwo_descriptor_0
dd 32
dd 1
dq _VN13InheritantTwo_descriptor

_VN14VirtualTypeTwo_descriptor_0:
db 'VirtualTypeTwo', 0, 1, 'InheritantTwo', 1, 2, 0

_VN15InheritantThree_configuration:
dq _VN15InheritantThree_descriptor

_VN15InheritantThree_descriptor:
dq _VN15InheritantThree_descriptor_0
dd 16
dd 0

_VN15InheritantThree_descriptor_0:
db 'InheritantThree', 0, 1, 2, 0

_VN16VirtualTypeThree_configuration:
dq _VN16VirtualTypeThree_descriptor
dq _VN16VirtualTypeThree_descriptor
dq _VN16VirtualTypeThree3bazEcs_rx_v

_VN16VirtualTypeThree_descriptor:
dq _VN16VirtualTypeThree_descriptor_0
dd 32
dd 1
dq _VN15InheritantThree_descriptor

_VN16VirtualTypeThree_descriptor_0:
db 'VirtualTypeThree', 0, 1, 'InheritantThree', 1, 2, 0

_VN15VirtualTypeFour_configuration:
dq _VN15VirtualTypeFour_descriptor
dq _VN15VirtualTypeFour_descriptor
dq _VN15VirtualTypeFour3fooEv_v
dq _VN15VirtualTypeFour_descriptor
dq _VN15VirtualTypeFour3barEv_v
dq _VN15VirtualTypeFour_descriptor
dq _VN15VirtualTypeFour3bazEcs_rx_v

_VN15VirtualTypeFour_descriptor:
dq _VN15VirtualTypeFour_descriptor_0
dd 48
dd 3
dq _VN13InheritantOne_descriptor
dq _VN13InheritantTwo_descriptor
dq _VN15InheritantThree_descriptor

_VN15VirtualTypeFour_descriptor_0:
db 'VirtualTypeFour', 0, 1, 'InheritantOne', 1, 'InheritantTwo', 1, 'InheritantThree', 1, 2, 0

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
align 16
_V9to_stringd_rP6String_S0 db ',0', 0
align 16
_V9to_stringd_rP6String_C0 db 0, 0, 0, 0, 0, 0, 0, 128, 0, 0, 0, 0, 0, 0, 0, 128
align 16
_V9to_stringd_rP6String_C1 db 0, 0, 0, 0, 0, 0, 36, 64 ; 10.0
align 16
_V9to_stringd_rP6String_C2 db 0, 0, 0, 0, 0, 0, 72, 64 ; 48.0