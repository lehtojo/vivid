section .text
global main
main:
jmp _V4initv_rx

extern _V8allocatex_rPh
extern _V4copyPhxPS_
extern _V11offset_copyPhxPS_x
extern _V14internal_printPhx

_V24execute_virtual_type_onev:
push rbx
sub rsp, 48
call _VN14VirtualTypeOne4initEv_rPS_
mov rcx, rax
mov rbx, rax
call _VN14VirtualTypeOne3fooEv
mov rcx, rbx
mov rdx, [rbx]
call qword [rdx+8]
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
mov rcx, rbx
mov rdx, [rbx]
call qword [rdx+8]
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
mov rcx, rbx
mov rdx, 255
mov r8, 32767
mov r9, [rbx]
call qword [r9+8]
mov rcx, rax
call _V7printlnx
mov rcx, rbx
mov rdx, 7
mov r8, 7
mov r9, [rbx]
call qword [r9+8]
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
mov rcx, rbx
mov rdx, [rbx]
call qword [rdx+8]
lea rcx, [rbx+8]
lea r8, [rbx+8]
mov rdx, [r8]
call qword [rdx+8]
lea rcx, [rbx+24]
xor rdx, rdx
mov r8, 1
lea r10, [rbx+24]
mov r9, [r10]
call qword [r9+8]
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
test rbx, rbx
jge _V9to_stringx_rP6String_L0
lea rcx, [rel _V9to_stringx_rP6String_S2]
call _VN6String4initEPh_rPS_
neg rbx
_V9to_stringx_rP6String_L0:
_V9to_stringx_rP6String_L3:
_V9to_stringx_rP6String_L2:
mov rdi, rax
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
test rbx, rbx
jne _V9to_stringx_rP6String_L5
mov rcx, rdi
mov rdx, rax
mov rsi, rax
call _VN6String7combineEPS__rS0_
add rsp, 40
pop rbp
pop rdi
pop rsi
pop rbx
ret
mov rax, rsi
_V9to_stringx_rP6String_L5:
mov rsi, rax
mov rax, rdi
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
sub rsp, 48
cvttsd2si rcx, xmm0
movsd qword [rsp+64], xmm0
call _V9to_stringx_rP6String
movsd xmm0, qword [rsp+64]
pxor xmm1, xmm1
comisd xmm0, xmm1
jae _V9to_stringd_rP6String_L0
xorpd xmm0, oword [rel _V9to_stringd_rP6String_C0]
_V9to_stringd_rP6String_L0:
cvttsd2si rcx, xmm0
cvtsi2sd xmm1, rcx
subsd xmm0, xmm1
pxor xmm1, xmm1
comisd xmm0, xmm1
jnz _V9to_stringd_rP6String_L2
lea rcx, [rel _V9to_stringd_rP6String_S0]
mov rbx, rax
movsd qword [rsp+64], xmm0
call _VN6String4initEPh_rPS_
mov rcx, rbx
mov rdx, rax
call _VN6String7combineEPS__rS0_
add rsp, 48
pop rbx
ret
movsd xmm0, qword [rsp+8]
mov rax, rbx
_V9to_stringd_rP6String_L2:
mov rcx, rax
mov rdx, 44
movsd qword [rsp+64], xmm0
call _VN6String6appendEh_rPS_
movsd xmm0, qword [rsp+64]
xor rcx, rcx
cmp rcx, 15
jge _V9to_stringd_rP6String_L5
pxor xmm1, xmm1
comisd xmm0, xmm1
jbe _V9to_stringd_rP6String_L5
_V9to_stringd_rP6String_L4:
movsd xmm1, qword [rel _V9to_stringd_rP6String_C1]
mulsd xmm0, xmm1
cvttsd2si rdx, xmm0
cvtsi2sd xmm1, rdx
subsd xmm0, xmm1
movsd xmm2, qword [rel _V9to_stringd_rP6String_C2]
addsd xmm2, xmm1
mov rbx, rcx
mov rcx, rax
cvttsd2si rdx, xmm2
movsd qword [rsp+64], xmm0
call _VN6String6appendEh_rPS_
add rbx, 1
mov rcx, rbx
movsd xmm0, qword [rsp+64]
cmp rcx, 15
jge _V9to_stringd_rP6String_L8
pxor xmm1, xmm1
comisd xmm0, xmm1
ja _V9to_stringd_rP6String_L4
_V9to_stringd_rP6String_L8:
_V9to_stringd_rP6String_L5:
add rsp, 48
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
mov rcx, 1
add rcx, 2
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
imul rdx, [rcx+8]
movsd xmm0, qword [rcx+24]
mulsd xmm0, qword [rcx+24]
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
sub rsp, 48
cmp rdx, r8
jle _VN16VirtualTypeThree3bazEcs_rx_L1
mov rcx, rdx
mov rbx, rdx
call _V7printlnx
mov rax, rbx
add rsp, 48
pop rbx
ret
mov rdx, rbx
jmp _VN16VirtualTypeThree3bazEcs_rx_L0
_VN16VirtualTypeThree3bazEcs_rx_L1:
cmp r8, rdx
jle _VN16VirtualTypeThree3bazEcs_rx_L3
mov rcx, r8
mov rbx, r8
call _V7printlnx
mov rax, rbx
add rsp, 48
pop rbx
ret
mov r8, rbx
jmp _VN16VirtualTypeThree3bazEcs_rx_L0
_VN16VirtualTypeThree3bazEcs_rx_L3:
movsd xmm0, qword [rcx+24]
mov rbx, rcx
call _V7printlnd
cvttsd2si rax, [rbx+24]
add rsp, 48
pop rbx
ret
mov rcx, rbx
_VN16VirtualTypeThree3bazEcs_rx_L0:
add rsp, 48
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
mov r9, rdx
cqo
idiv qword [rcx+32]
mov rcx, rax
mov rax, r9
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
add rax, 1
lea rcx, [rdi+rax]
mov rbp, rax
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
mov rcx, rbp
sub rcx, rsi
lea r9, [rsi+1]
mov rdx, rcx
mov rcx, [rbx+8]
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
mov r8, [rcx+8]
movzx rdx, byte [r8+rax]
test rdx, rdx
je _VN6String6lengthEv_rx_L1
_VN6String6lengthEv_rx_L0:
add rax, 1
mov r8, [rcx+8]
movzx rdx, byte [r8+rax]
test rdx, rdx
jne _VN6String6lengthEv_rx_L0
_VN6String6lengthEv_rx_L1:
ret

section .data

_VN13InheritantOne_configuration:
dq _VN13InheritantOne_descriptor

_VN13InheritantOne_descriptor:
dq _VN13InheritantOne_descriptor_0
dd 8
dd 0

_VN13InheritantOne_descriptor_0:
db 'InheritantOne', 0

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
db 'VirtualTypeOne', 0

_VN13InheritantTwo_configuration:
dq _VN13InheritantTwo_descriptor

_VN13InheritantTwo_descriptor:
dq _VN13InheritantTwo_descriptor_0
dd 16
dd 0

_VN13InheritantTwo_descriptor_0:
db 'InheritantTwo', 0

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
db 'VirtualTypeTwo', 0

_VN15InheritantThree_configuration:
dq _VN15InheritantThree_descriptor

_VN15InheritantThree_descriptor:
dq _VN15InheritantThree_descriptor_0
dd 16
dd 0

_VN15InheritantThree_descriptor_0:
db 'InheritantThree', 0

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
db 'VirtualTypeThree', 0

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
db 'VirtualTypeFour', 0

_VN6String_configuration:
dq _VN6String_descriptor

_VN6String_descriptor:
dq _VN6String_descriptor_0
dd 16
dd 0

_VN6String_descriptor_0:
db 'String', 0

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