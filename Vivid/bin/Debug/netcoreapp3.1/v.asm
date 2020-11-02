section .text
global main
main:
jmp _V4initv_rx

extern _V8allocatex_rPh
extern _V4copyPhxPS_
extern _V11offset_copyPhxPS_x
extern _V4sqrtd_rd
extern _V3powdd_rd
extern _V14internal_printPhx

_V21create_default_actionv_rPFvE_0_:
sub rsp, 40
mov rdx, rcx
lea rcx, [rel _V21create_default_actionv_rPFvE_0__S0]
call _V7printlnPh
add rsp, 40
ret

global _V21create_default_actionv_rPFvE
export _V21create_default_actionv_rPFvE
_V21create_default_actionv_rPFvE:
sub rsp, 40
mov rcx, 8
call _V8allocatex_rPh
lea rcx, [rel _V21create_default_actionv_rPFvE_0_]
mov qword [rax], rcx
add rsp, 40
ret

global _V22execute_default_actionPFvE
export _V22execute_default_actionPFvE
_V22execute_default_actionPFvE:
sub rsp, 40
call qword [rcx]
add rsp, 40
ret

_V20create_number_actionv_rPFvxE_0_x:
sub rsp, 40
mov r8, rcx
mov rcx, rdx
call _V9to_stringx_rP6String
mov rcx, rax
call _V7printlnP6String
add rsp, 40
ret

global _V20create_number_actionv_rPFvxE
export _V20create_number_actionv_rPFvxE
_V20create_number_actionv_rPFvxE:
sub rsp, 40
mov rcx, 8
call _V8allocatex_rPh
lea rcx, [rel _V20create_number_actionv_rPFvxE_0_x]
mov qword [rax], rcx
add rsp, 40
ret

global _V21execute_number_actionPFvxEx
export _V21execute_number_actionPFvxEx
_V21execute_number_actionPFvxEx:
sub rsp, 40
call qword [rcx]
add rsp, 40
ret

_V19create_sum_functionv_rPFxxxE_0_xx_rx:
add rdx, r8
mov rax, rdx
ret

global _V19create_sum_functionv_rPFxxxE
export _V19create_sum_functionv_rPFxxxE
_V19create_sum_functionv_rPFxxxE:
sub rsp, 40
mov rcx, 8
call _V8allocatex_rPh
lea rcx, [rel _V19create_sum_functionv_rPFxxxE_0_xx_rx]
mov qword [rax], rcx
add rsp, 40
ret

global _V20execute_sum_functionPFxxxExx_rx
export _V20execute_sum_functionPFxxxExx_rx
_V20execute_sum_functionPFxxxExx_rx:
sub rsp, 40
call qword [rcx]
add rsp, 40
ret

_V25create_capturing_functioncsixd_rPFdE_0__rd:
movsx rdx, byte [rcx+8]
movsx r8, word [rcx+9]
add rdx, r8
movsxd r8, dword [rcx+11]
add rdx, r8
add rdx, [rcx+15]
cvtsi2sd xmm0, rdx
addsd xmm0, qword [rcx+23]
ret

global _V25create_capturing_functioncsixd_rPFdE
export _V25create_capturing_functioncsixd_rPFdE
_V25create_capturing_functioncsixd_rPFdE:
push rbx
push rsi
push rdi
push rbp
sub rsp, 40
mov rbx, rcx
mov rcx, 31
mov rsi, rdx
movsd qword [rsp+95], xmm0
mov rdi, r8
mov rbp, r9
call _V8allocatex_rPh
lea rcx, [rel _V25create_capturing_functioncsixd_rPFdE_0__rd]
mov qword [rax], rcx
mov byte [rax+8], bl
mov word [rax+9], si
mov dword [rax+11], edi
mov qword [rax+15], rbp
movsd xmm0, qword [rsp+95]
movsd qword [rax+23], xmm0
add rsp, 40
pop rbp
pop rdi
pop rsi
pop rbx
ret

global _V26execute_capturing_functionPFdE_rd
export _V26execute_capturing_functionPFdE_rd
_V26execute_capturing_functionPFdE_rd:
sub rsp, 40
call qword [rcx]
add rsp, 40
ret

_V40create_capturing_function_with_parameterP3DogP3Cat_rPFvdE_0_d:
push rbx
sub rsp, 48
mov rbx, rcx
mov rcx, 1
mov rdx, 1
movsd qword [rsp+64], xmm0
call _VN6Vector4initExx_rPS_
movsd xmm0, qword [rsp+64]
movsd xmm1, xmm0
movsd xmm2, qword [rel _V40create_capturing_function_with_parameterP3DogP3Cat_rPFvdE_0_d_C0]
mulsd xmm1, xmm2
mov rcx, rax
movsd xmm2, xmm0
movsd xmm0, xmm1
movsd qword [rsp+64], xmm2
call _VN6Vector5timesEd_rPS_
mov rcx, [rbx+8]
mov qword [rcx+24], rax
mov rcx, 1
mov rdx, 1
call _VN6Vector4initExx_rPS_
movsd xmm0, qword [rsp+64]
movsd xmm1, qword [rel _V40create_capturing_function_with_parameterP3DogP3Cat_rPFvdE_0_d_C0]
mulsd xmm0, xmm1
xorpd xmm0, oword [rel _V40create_capturing_function_with_parameterP3DogP3Cat_rPFvdE_0_d_C1]
mov rcx, rax
call _VN6Vector5timesEd_rPS_
mov rcx, [rbx+16]
mov qword [rcx+24], rax
mov rcx, [rbx+8]
mov rdx, [rbx+16]
call _VN6Animal8interactEPS_
mov rcx, [rbx+16]
mov rdx, [rbx+8]
call _VN6Animal8interactEPS_
add rsp, 48
pop rbx
ret

global _V40create_capturing_function_with_parameterP3DogP3Cat_rPFvdE
export _V40create_capturing_function_with_parameterP3DogP3Cat_rPFvdE
_V40create_capturing_function_with_parameterP3DogP3Cat_rPFvdE:
push rbx
push rsi
sub rsp, 40
mov rbx, rcx
mov rcx, 24
mov rsi, rdx
call _V8allocatex_rPh
lea rcx, [rel _V40create_capturing_function_with_parameterP3DogP3Cat_rPFvdE_0_d]
mov qword [rax], rcx
mov qword [rax+8], rbx
mov qword [rax+16], rsi
add rsp, 40
pop rsi
pop rbx
ret

global _V41execute_capturing_function_with_parameterPFvdEd
export _V41execute_capturing_function_with_parameterPFvdEd
_V41execute_capturing_function_with_parameterPFvdEd:
sub rsp, 40
call qword [rcx]
add rsp, 40
ret

_V4initv_rx:
push rbx
push rsi
sub rsp, 40
call _VN3Dog4initEv_rPS_
mov rbx, rax
call _VN3Cat4initEv_rPS_
mov rcx, 10
mov rdx, 10
mov rsi, rax
call _VN6Vector4initExx_rPS_
mov qword [rbx+24], rax
xor rcx, rcx
xor rdx, rdx
call _VN6Vector4initExx_rPS_
mov qword [rsi+24], rax
mov rcx, rbx
mov rdx, rsi
call _VN6Animal8interactEPS_
mov rcx, rsi
mov rdx, rbx
call _VN6Animal8interactEPS_
movsd xmm0, qword [rel _V4initv_rx_C0]
movsd xmm1, qword [rel _V4initv_rx_C0]
call _VN6Vector4initEdd_rPS_
mov qword [rbx+24], rax
xor rcx, rcx
xor rdx, rdx
call _VN6Vector4initExx_rPS_
mov qword [rsi+24], rax
mov rcx, rbx
mov rdx, rsi
call _VN6Animal8interactEPS_
mov rcx, rsi
mov rdx, rbx
call _VN6Animal8interactEPS_
call _V21create_default_actionv_rPFvE
mov rcx, rax
call _V22execute_default_actionPFvE
call _V20create_number_actionv_rPFvxE
mov rcx, rax
mov rdx, -1
call _V21execute_number_actionPFvxEx
call _V19create_sum_functionv_rPFxxxE
mov rcx, rax
mov rdx, 1
mov r8, 2
call _V20execute_sum_functionPFxxxExx_rx
mov rcx, rax
call _V7printlnx
movsd xmm0, qword [rel _V4initv_rx_C1]
mov rcx, 1
mov rdx, 2
mov r8, 3
mov r9, 4
call _V25create_capturing_functioncsixd_rPFdE
mov rcx, rax
call _V26execute_capturing_functionPFdE_rd
call _V7printlnd
mov rcx, rbx
mov rdx, rsi
call _V40create_capturing_function_with_parameterP3DogP3Cat_rPFvdE
mov rcx, rax
movsd xmm0, qword [rel _V4initv_rx_C2]
mov rbx, rax
call _V41execute_capturing_function_with_parameterPFvdEd
mov rcx, rbx
movsd xmm0, qword [rel _V4initv_rx_C3]
call _V41execute_capturing_function_with_parameterPFvdEd
xor rax, rax
add rsp, 40
pop rsi
pop rbx
ret

_V5printP6String:
push rbx
sub rsp, 48
mov rbx, rcx
call _VN6String4dataEv_rPh
mov rcx, rbx
mov rbx, rax
call _VN6String6lengthEv_rx
mov rcx, rbx
mov rdx, rax
call _V14internal_printPhx
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

_V7printlnPh:
sub rsp, 40
call _VN6String4initEPh_rPS_
mov rcx, rax
mov rdx, 10
call _VN6String6appendEh_rPS_
mov rcx, rax
call _V5printP6String
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

_VN6Vector4initExx_rPS_:
push rbx
push rsi
sub rsp, 40
mov rbx, rcx
mov rcx, 24
mov rsi, rdx
call _V8allocatex_rPh
cvtsi2sd xmm0, rbx
movsd qword [rax+8], xmm0
cvtsi2sd xmm0, rsi
movsd qword [rax+16], xmm0
add rsp, 40
pop rsi
pop rbx
ret

_VN6Vector4initEdd_rPS_:
push rbx
sub rsp, 48
mov rcx, 24
movsd qword [rsp+64], xmm0
movsd qword [rsp+72], xmm1
call _V8allocatex_rPh
mov rcx, 24
mov rbx, rax
call _V8allocatex_rPh
movsd xmm0, qword [rsp+64]
movsd qword [rbx+8], xmm0
movsd xmm0, qword [rsp+72]
movsd qword [rbx+16], xmm0
mov rax, rbx
add rsp, 48
pop rbx
ret

_VN6Vector6invertEv:
movsd xmm0, qword [rcx+8]
xorpd xmm0, oword [rel _VN6Vector6invertEv_C0]
movsd qword [rcx+8], xmm0
movsd xmm0, qword [rcx+16]
xorpd xmm0, oword [rel _VN6Vector6invertEv_C1]
movsd qword [rcx+16], xmm0
ret

_VN6Vector5minusEPS__rS0_:
sub rsp, 40
movsd xmm0, qword [rcx+8]
subsd xmm0, qword [rdx+8]
movsd xmm1, qword [rcx+16]
subsd xmm1, qword [rdx+16]
call _VN6Vector4initEdd_rPS_
add rsp, 40
ret

_VN6Vector5timesEd_rPS_:
sub rsp, 40
movsd xmm1, qword [rcx+8]
mulsd xmm1, xmm0
movsd xmm2, qword [rcx+16]
mulsd xmm2, xmm0
movsd xmm0, xmm1
movsd xmm1, xmm2
call _VN6Vector4initEdd_rPS_
add rsp, 40
ret

_VN6Vector5timesEx_rPS_:
sub rsp, 40
movsd xmm0, qword [rcx+8]
cvtsi2sd xmm1, rdx
mulsd xmm0, xmm1
movsd xmm2, qword [rcx+16]
mulsd xmm2, xmm1
movsd xmm1, xmm2
call _VN6Vector4initEdd_rPS_
add rsp, 40
ret

_VN6Vector11assign_plusEPS_:
movsd xmm0, qword [rcx+8]
addsd xmm0, qword [rdx+8]
movsd qword [rcx+8], xmm0
movsd xmm0, qword [rcx+16]
addsd xmm0, qword [rdx+16]
movsd qword [rcx+16], xmm0
ret

_VN6Vector12assign_timesEx:
movsd xmm0, qword [rcx+8]
cvtsi2sd xmm1, rdx
mulsd xmm0, xmm1
movsd qword [rcx+8], xmm0
movsd xmm0, qword [rcx+16]
mulsd xmm0, xmm1
movsd qword [rcx+16], xmm0
ret

_VN6Animal8interactEPS_:
push rbx
sub rsp, 48
mov rbx, rcx
mov rcx, [rbx+8]
mov r8, [rbx+8]
call qword [r8]
mov rcx, rbx
mov rdx, rax
call _VN6Animal4moveEP6Vector
add rsp, 48
pop rbx
ret

_VN6Animal4moveEP6Vector:
sub rsp, 40
mov r8, rcx
mov rcx, [r8+24]
call _VN6Vector11assign_plusEPS_
add rsp, 40
ret

_VN3Dog4initEv_rPS__0_P6Animal_rP6Vector:
push rbx
push rsi
sub rsp, 72
mov r9, [rcx+8]
mov r8, [r9+24]
movsd xmm0, qword [r8+8]
mov r10, [rdx+24]
subsd xmm0, qword [r10+8]
movsd xmm1, qword [rel _VN3Dog4initEv_rPS__0_P6Animal_rP6Vector_C0]
mov rbx, rcx
mov rsi, rdx
call _V3powdd_rd
mov rdx, [rbx+8]
mov rcx, [rdx+24]
movsd xmm1, qword [rcx+16]
mov r8, [rsi+24]
subsd xmm1, qword [r8+16]
movsd xmm2, xmm0
movsd xmm0, xmm1
movsd xmm1, qword [rel _VN3Dog4initEv_rPS__0_P6Animal_rP6Vector_C0]
movsd qword [rsp+56], xmm2
call _V3powdd_rd
movsd xmm1, qword [rsp+56]
addsd xmm1, xmm0
movsd xmm0, xmm1
call _V4sqrtd_rd
mov rcx, [rsi+24]
mov r8, [rbx+8]
mov rdx, [r8+24]
movsd qword [rsp+64], xmm0
call _VN6Vector5minusEPS__rS0_
movsd xmm0, qword [rsp+64]
movsd xmm1, qword [rel _VN3Dog4initEv_rPS__0_P6Animal_rP6Vector_C1]
comisd xmm0, xmm1
ja _VN3Dog4initEv_rPS__0_P6Animal_rP6Vector_L1
mov rcx, [rsi+16]
cmp rcx, 1
jne _VN3Dog4initEv_rPS__0_P6Animal_rP6Vector_L1
mov rcx, rax
mov rbx, rax
call _VN6Vector6invertEv
mov rcx, rbx
mov rdx, 10
call _VN6Vector12assign_timesEx
mov rcx, [rbx+8]
call _VN3Dog4barkEv
mov rcx, [rbx+8]
call _VN3Dog4barkEv
mov rax, rbx
jmp _VN3Dog4initEv_rPS__0_P6Animal_rP6Vector_L0
_VN3Dog4initEv_rPS__0_P6Animal_rP6Vector_L1:
mov rcx, [rbx+8]
mov rbx, rax
call _VN3Dog4barkEv
mov rax, rbx
_VN3Dog4initEv_rPS__0_P6Animal_rP6Vector_L0:
add rsp, 72
pop rsi
pop rbx
ret

_VN3Dog4initEv_rPS_:
push rbx
sub rsp, 48
mov rcx, 40
call _V8allocatex_rPh
lea rcx, [rel _VN3Dog_configuration+8]
mov qword [rax], rcx
xor rcx, rcx
xor rdx, rdx
mov rbx, rax
call _VN6Vector4initExx_rPS_
mov qword [rbx+24], rax
mov qword [rbx+16], 0
mov rcx, 16
call _V8allocatex_rPh
lea rcx, [rel _VN3Dog4initEv_rPS__0_P6Animal_rP6Vector]
mov qword [rax], rcx
mov qword [rax+8], rbx
mov qword [rbx+8], rax
mov rax, rbx
add rsp, 48
pop rbx
ret

_VN3Dog4barkEv:
sub rsp, 40
lea rcx, [rel _VN3Dog4barkEv_S0]
call _V7printlnPh
add rsp, 40
ret

_VN3Cat4initEv_rPS__0_P6Animal_rP6Vector:
push rbx
push rsi
sub rsp, 40
mov rbx, rcx
mov rcx, [rbx+8]
mov rsi, rdx
call _VN3Cat4meowEv
mov rcx, [rsi+24]
mov r8, [rbx+8]
mov rdx, [r8+24]
call _VN6Vector5minusEPS__rS0_
mov rcx, rax
mov rdx, 2
call _VN6Vector5timesEx_rPS_
add rsp, 40
pop rsi
pop rbx
ret

_VN3Cat4initEv_rPS_:
push rbx
sub rsp, 48
mov rcx, 40
call _V8allocatex_rPh
lea rcx, [rel _VN3Cat_configuration+8]
mov qword [rax], rcx
xor rcx, rcx
xor rdx, rdx
mov rbx, rax
call _VN6Vector4initExx_rPS_
mov qword [rbx+24], rax
mov qword [rbx+16], 1
mov rcx, 16
call _V8allocatex_rPh
lea rcx, [rel _VN3Cat4initEv_rPS__0_P6Animal_rP6Vector]
mov qword [rax], rcx
mov qword [rax+8], rbx
mov qword [rbx+8], rax
mov rax, rbx
add rsp, 48
pop rbx
ret

_VN3Cat4meowEv:
sub rsp, 40
lea rcx, [rel _VN3Cat4meowEv_S0]
call _V7printlnPh
add rsp, 40
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

_VN6Vector_configuration:
dq _VN6Vector_descriptor

_VN6Vector_descriptor:
dq _VN6Vector_descriptor_0
dd 24
dd 0

_VN6Vector_descriptor_0:
db 'Vector', 0

_VN6Animal_configuration:
dq _VN6Animal_descriptor

_VN6Animal_descriptor:
dq _VN6Animal_descriptor_0
dd 32
dd 0

_VN6Animal_descriptor_0:
db 'Animal', 0

_VN3Dog_configuration:
dq _VN3Dog_descriptor
dq _VN3Dog_descriptor

_VN3Dog_descriptor:
dq _VN3Dog_descriptor_0
dd 40
dd 1
dq _VN6Animal_descriptor

_VN3Dog_descriptor_0:
db 'Dog', 0

_VN3Cat_configuration:
dq _VN3Cat_descriptor
dq _VN3Cat_descriptor

_VN3Cat_descriptor:
dq _VN3Cat_descriptor_0
dd 40
dd 1
dq _VN6Animal_descriptor

_VN3Cat_descriptor_0:
db 'Cat', 0

_VN6String_configuration:
dq _VN6String_descriptor

_VN6String_descriptor:
dq _VN6String_descriptor_0
dd 16
dd 0

_VN6String_descriptor_0:
db 'String', 0

_VN14TypeDescriptor_configuration:
dq _VN14TypeDescriptor_descriptor

_VN14TypeDescriptor_descriptor:
dq _VN14TypeDescriptor_descriptor_0
dd 16
dd 0

_VN14TypeDescriptor_descriptor_0:
db 'TypeDescriptor', 0

_VN5Array_configuration:
dq _VN5Array_descriptor

_VN5Array_descriptor:
dq _VN5Array_descriptor_0
dd 8
dd 0

_VN5Array_descriptor_0:
db 'Array', 0

_VN5Sheet_configuration:
dq _VN5Sheet_descriptor

_VN5Sheet_descriptor:
dq _VN5Sheet_descriptor_0
dd 8
dd 0

_VN5Sheet_descriptor_0:
db 'Sheet', 0

_VN3Box_configuration:
dq _VN3Box_descriptor

_VN3Box_descriptor:
dq _VN3Box_descriptor_0
dd 8
dd 0

_VN3Box_descriptor_0:
db 'Box', 0

align 16
_V21create_default_actionv_rPFvE_0__S0 db 'Hi there!', 0
align 16
_V9to_stringx_rP6String_S0 db 0
align 16
_V9to_stringx_rP6String_S1 db 0
align 16
_V9to_stringx_rP6String_S2 db '-', 0
align 16
_V9to_stringd_rP6String_S0 db ',0', 0
align 16
_VN3Dog4barkEv_S0 db 'Bark', 0
align 16
_VN3Cat4meowEv_S0 db 'Meow', 0
align 16
_V40create_capturing_function_with_parameterP3DogP3Cat_rPFvdE_0_d_C0 db 0, 0, 0, 0, 0, 0, 224, 63 ; 0.5
align 16
_V40create_capturing_function_with_parameterP3DogP3Cat_rPFvdE_0_d_C1 db 0, 0, 0, 0, 0, 0, 0, 128, 0, 0, 0, 0, 0, 0, 0, 128
align 16
_V4initv_rx_C0 db 0, 0, 0, 0, 0, 0, 224, 63 ; 0.5
align 16
_V4initv_rx_C1 db 0, 0, 0, 0, 0, 0, 20, 64 ; 5.0
align 16
_V4initv_rx_C2 db 57, 180, 200, 118, 190, 159, 246, 63 ; 1.414
align 16
_V4initv_rx_C3 db 154, 153, 153, 153, 153, 153, 185, 191 ; -0.1
align 16
_V9to_stringd_rP6String_C0 db 0, 0, 0, 0, 0, 0, 0, 128, 0, 0, 0, 0, 0, 0, 0, 128
align 16
_V9to_stringd_rP6String_C1 db 0, 0, 0, 0, 0, 0, 36, 64 ; 10.0
align 16
_V9to_stringd_rP6String_C2 db 0, 0, 0, 0, 0, 0, 72, 64 ; 48.0
align 16
_VN6Vector6invertEv_C0 db 0, 0, 0, 0, 0, 0, 0, 128, 0, 0, 0, 0, 0, 0, 0, 128
align 16
_VN6Vector6invertEv_C1 db 0, 0, 0, 0, 0, 0, 0, 128, 0, 0, 0, 0, 0, 0, 0, 128
align 16
_VN3Dog4initEv_rPS__0_P6Animal_rP6Vector_C0 db 0, 0, 0, 0, 0, 0, 0, 64 ; 2.0
align 16
_VN3Dog4initEv_rPS__0_P6Animal_rP6Vector_C1 db 0, 0, 0, 0, 0, 0, 240, 63 ; 1.0