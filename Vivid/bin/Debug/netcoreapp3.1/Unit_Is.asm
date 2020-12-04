.section .text
.intel_syntax noprefix
.file 1 "Sandbox.v"
.global main
main:
jmp _V4initv_rx

.extern _V4copyPhxS_
.extern _V4exitx
.extern _V17internal_allocatex_rPh
.extern _V4sqrtd_rd
.extern _V14internal_printPhx

.global _V7can_useP6EntityP6Usable_rb
_V7can_useP6EntityP6Usable_rb:
push rbx
push rsi
push rdi
push rbp
push r12
push r13
push r14
push r15
sub rsp, 40
mov r12, rdx
xor r13, r13
mov r14, rcx
xor r15, r15
mov rcx, [r12]
mov r8, [rcx+8]
mov rcx, r12
mov rdx, r14
call r8
movzx rax, al
xor rax, 1
test rax, rax
jz _V7can_useP6EntityP6Usable_rb_L1
xor rax, rax
add rsp, 40
pop r15
pop r14
pop r13
pop r12
pop rbp
pop rdi
pop rsi
pop rbx
ret
jmp _V7can_useP6EntityP6Usable_rb_L0
_V7can_useP6EntityP6Usable_rb_L1:
mov rbp, r12
mov rcx, [r12]
mov rsi, [rcx]
lea rcx, [rip+_VN7Vehicle_descriptor]
cmp rsi, rcx
jz _V7can_useP6EntityP6Usable_rb_L6
mov rcx, rsi
lea rdx, [rip+_VN7Vehicle_descriptor]
call _V8inheritsPhS__rx
test rax, rax
je _V7can_useP6EntityP6Usable_rb_L5
_V7can_useP6EntityP6Usable_rb_L6:
mov r13, rbp
_V7can_useP6EntityP6Usable_rb_L5:
test r13, r13
jz _V7can_useP6EntityP6Usable_rb_L0
mov rdi, r14
mov rcx, [r14]
mov rbx, [rcx]
lea rcx, [rip+_VN6Person_descriptor]
cmp rbx, rcx
jz _V7can_useP6EntityP6Usable_rb_L9
mov rcx, rbx
lea rdx, [rip+_VN6Person_descriptor]
call _V8inheritsPhS__rx
test rax, rax
je _V7can_useP6EntityP6Usable_rb_L8
_V7can_useP6EntityP6Usable_rb_L9:
mov r15, rdi
_V7can_useP6EntityP6Usable_rb_L8:
test r15, r15
jz _V7can_useP6EntityP6Usable_rb_L0
xor rbx, rbx
mov rsi, [r15+24]
mov rcx, [r13+8]
mov rdx, [rcx+8]
mov rcx, r13
call rdx
cmp rsi, rax
jl _V7can_useP6EntityP6Usable_rb_L11
mov rbx, 1
_V7can_useP6EntityP6Usable_rb_L11:
mov rax, rbx
add rsp, 40
pop r15
pop r14
pop r13
pop r12
pop rbp
pop rdi
pop rsi
pop rbx
ret
_V7can_useP6EntityP6Usable_rb_L0:
xor rax, rax
add rsp, 40
pop r15
pop r14
pop r13
pop r12
pop rbp
pop rdi
pop rsi
pop rbx
ret

.global _V21get_reliable_vehiclesP5ArrayIP6UsableEx_rP4ListIP7VehicleE
_V21get_reliable_vehiclesP5ArrayIP6UsableEx_rP4ListIP7VehicleE:
push rbx
push rsi
push rdi
push rbp
push r12
sub rsp, 32
mov rbx, rcx
mov rsi, rdx
call _VN4ListIP7VehicleE4initEv_rS1_
xor rdi, rdi
mov rbp, rax
mov rcx, [rbx+16]
cmp rdi, rcx
jge _V21get_reliable_vehiclesP5ArrayIP6UsableEx_rP4ListIP7VehicleE_L1
_V21get_reliable_vehiclesP5ArrayIP6UsableEx_rP4ListIP7VehicleE_L0:
mov rcx, rbx
mov rdx, rdi
call _VN5ArrayIP6UsableE3getEx_rS0_
mov rcx, [rax]
mov rdx, [rcx]
lea r8, [rip+_VN7Vehicle_descriptor]
cmp rdx, r8
jz _V21get_reliable_vehiclesP5ArrayIP6UsableEx_rP4ListIP7VehicleE_L4
mov rcx, rbx
mov rdx, rdi
call _VN5ArrayIP6UsableE3getEx_rS0_
mov rcx, [rax]
mov rdx, [rcx]
mov rcx, rdx
lea rdx, [rip+_VN7Vehicle_descriptor]
call _V8inheritsPhS__rx
test rax, rax
je _V21get_reliable_vehiclesP5ArrayIP6UsableEx_rP4ListIP7VehicleE_L3
_V21get_reliable_vehiclesP5ArrayIP6UsableEx_rP4ListIP7VehicleE_L4:
mov rcx, rbx
mov rdx, rdi
call _VN5ArrayIP6UsableE3getEx_rS0_
mov rcx, rbp
mov rdx, rax
call _VN4ListIP7VehicleE3addES0_
_V21get_reliable_vehiclesP5ArrayIP6UsableEx_rP4ListIP7VehicleE_L3:
add rdi, 1
mov rcx, [rbx+16]
cmp rdi, rcx
jl _V21get_reliable_vehiclesP5ArrayIP6UsableEx_rP4ListIP7VehicleE_L0
_V21get_reliable_vehiclesP5ArrayIP6UsableEx_rP4ListIP7VehicleE_L1:
mov rcx, rbp
call _VN4ListIP7VehicleE4sizeEv_rx
sub rax, 1
mov rdi, rax
test rdi, rdi
jl _V21get_reliable_vehiclesP5ArrayIP6UsableEx_rP4ListIP7VehicleE_L8
_V21get_reliable_vehiclesP5ArrayIP6UsableEx_rP4ListIP7VehicleE_L7:
mov rcx, rbp
mov rdx, rdi
call _VN4ListIP7VehicleE3getEx_rS0_
mov rcx, rbp
mov rdx, rdi
mov r12, rax
call _VN4ListIP7VehicleE3getEx_rS0_
mov rcx, [rax+8]
mov rdx, [rcx+16]
mov rcx, r12
call rdx
cmp rax, rsi
jge _V21get_reliable_vehiclesP5ArrayIP6UsableEx_rP4ListIP7VehicleE_L10
mov rcx, rbp
mov rdx, rdi
call _VN4ListIP7VehicleE6removeEx
_V21get_reliable_vehiclesP5ArrayIP6UsableEx_rP4ListIP7VehicleE_L10:
sub rdi, 1
test rdi, rdi
jge _V21get_reliable_vehiclesP5ArrayIP6UsableEx_rP4ListIP7VehicleE_L7
_V21get_reliable_vehiclesP5ArrayIP6UsableEx_rP4ListIP7VehicleE_L8:
mov rax, rbp
add rsp, 32
pop r12
pop rbp
pop rdi
pop rsi
pop rbx
ret

.global _V14choose_vehicleP6EntityP4ListIP7VehicleEx_rS2_
_V14choose_vehicleP6EntityP4ListIP7VehicleEx_rS2_:
sub rsp, 40
cvtsi2sd xmm0, r8
call _V14choose_vehicleP6EntityP4ListIP7VehicleEd_rS2_
add rsp, 40
ret

.global _V14choose_vehicleP6EntityP4ListIP7VehicleEd_rS2_
_V14choose_vehicleP6EntityP4ListIP7VehicleEd_rS2_:
push rbx
push rsi
push rdi
push rbp
push r12
push r13
push r14
sub rsp, 48
xor rdi, rdi
mov rbp, rcx
mov rsi, rbp
mov rcx, [rbp]
mov rbx, [rcx]
lea rcx, [rip+_VN6Person_descriptor]
cmp rbx, rcx
jz _V14choose_vehicleP6EntityP4ListIP7VehicleEd_rS2__L4
mov rcx, rbx
mov r12, rdx
lea rdx, [rip+_VN6Person_descriptor]
movsd qword ptr [rsp+128], xmm0
call _V8inheritsPhS__rx
test rax, rax
mov rdx, r12
movsd xmm0, qword ptr [rsp+128]
je _V14choose_vehicleP6EntityP4ListIP7VehicleEd_rS2__L3
_V14choose_vehicleP6EntityP4ListIP7VehicleEd_rS2__L4:
mov rdi, rsi
_V14choose_vehicleP6EntityP4ListIP7VehicleEd_rS2__L3:
test rdi, rdi
jz _V14choose_vehicleP6EntityP4ListIP7VehicleEd_rS2__L0
mov rcx, [rdi+16]
mov rbx, rdx
lea rdx, [rip+_V14choose_vehicleP6EntityP4ListIP7VehicleEd_rS2__S0]
movsd qword ptr [rsp+128], xmm0
call _VN6String6equalsEPh_rb
movzx rax, al
test rax, rax
mov rdx, rbx
movsd xmm0, qword ptr [rsp+128]
jz _V14choose_vehicleP6EntityP4ListIP7VehicleEd_rS2__L0
mov rbx, rdx
movsd qword ptr [rsp+128], xmm0
call _VN3Pig4initEv_rPS_
lea rax, [rax+8]
add rsp, 48
pop r14
pop r13
pop r12
pop rbp
pop rdi
pop rsi
pop rbx
ret
mov rdx, rbx
movsd xmm0, qword ptr [rsp+24]
_V14choose_vehicleP6EntityP4ListIP7VehicleEd_rS2__L0:
mov rcx, rdx
mov r12, rdx
xor rdx, rdx
movsd qword ptr [rsp+128], xmm0
call _VN4ListIP7VehicleE3getEx_rS0_
mov rcx, r12
xor rdx, rdx
mov r13, rax
call _VN4ListIP7VehicleE3getEx_rS0_
mov rcx, rax
movsd xmm0, qword ptr [rsp+128]
call _VN7Vehicle4timeEd_rd
mov r14, 1
mov rcx, r12
movsd qword ptr [rsp+40], xmm0
call _VN4ListIP7VehicleE4sizeEv_rx
cmp r14, rax
movsd xmm0, qword ptr [rsp+40]
jge _V14choose_vehicleP6EntityP4ListIP7VehicleEd_rS2__L7
_V14choose_vehicleP6EntityP4ListIP7VehicleEd_rS2__L6:
mov rcx, r12
mov rdx, r14
movsd qword ptr [rsp+40], xmm0
call _VN4ListIP7VehicleE3getEx_rS0_
mov rcx, rax
movsd xmm0, qword ptr [rsp+128]
mov rbx, rax
call _VN7Vehicle4timeEd_rd
movsd xmm1, qword ptr [rsp+40]
comisd xmm0, xmm1
jae _V14choose_vehicleP6EntityP4ListIP7VehicleEd_rS2__L9
mov r13, rbx
movsd xmm1, xmm0
_V14choose_vehicleP6EntityP4ListIP7VehicleEd_rS2__L9:
add r14, 1
movsd qword ptr [rsp+40], xmm1
mov rcx, r12
call _VN4ListIP7VehicleE4sizeEv_rx
cmp r14, rax
movsd xmm0, qword ptr [rsp+40]
jl _V14choose_vehicleP6EntityP4ListIP7VehicleEd_rS2__L6
_V14choose_vehicleP6EntityP4ListIP7VehicleEd_rS2__L7:
mov rax, r13
add rsp, 48
pop r14
pop r13
pop r12
pop rbp
pop rdi
pop rsi
pop rbx
ret

.global _V10create_pigv_rP3Pig
_V10create_pigv_rP3Pig:
sub rsp, 40
call _VN3Pig4initEv_rPS_
add rsp, 40
ret

.global _V10create_busv_rP3Bus
_V10create_busv_rP3Bus:
sub rsp, 40
call _VN3Bus4initEv_rPS_
add rsp, 40
ret

.global _V10create_carv_rP3Car
_V10create_carv_rP3Car:
sub rsp, 40
call _VN3Car4initEv_rPS_
add rsp, 40
ret

.global _V13create_bananav_rP6Banana
_V13create_bananav_rP6Banana:
sub rsp, 40
call _VN6Banana4initEv_rPS_
add rsp, 40
ret

.global _V11create_johnv_rP6Person
_V11create_johnv_rP6Person:
sub rsp, 40
lea rcx, [rip+_V11create_johnv_rP6Person_S0]
mov rdx, 10
mov r8, 1
xor r9, r9
call _VN6Person4initEPhxbb_rPS_
add rsp, 40
ret

.global _V10create_maxv_rP6Person
_V10create_maxv_rP6Person:
sub rsp, 40
lea rcx, [rip+_V10create_maxv_rP6Person_S0]
mov rdx, 7
mov r8, 1
mov r9, 1
call _VN6Person4initEPhxbb_rPS_
add rsp, 40
ret

.global _V11create_gabev_rP6Person
_V11create_gabev_rP6Person:
sub rsp, 40
lea rcx, [rip+_V11create_gabev_rP6Person_S0]
mov rdx, 50
mov r8, 1
xor r9, r9
call _VN6Person4initEPhxbb_rPS_
add rsp, 40
ret

.global _V12create_stevev_rP6Person
_V12create_stevev_rP6Person:
sub rsp, 40
lea rcx, [rip+_V12create_stevev_rP6Person_S0]
mov rdx, 1
xor r8, r8
mov r9, 1
call _VN6Person4initEPhxbb_rPS_
add rsp, 40
ret

.global _V12create_arrayx_rP5ArrayIP6UsableE
_V12create_arrayx_rP5ArrayIP6UsableE:
sub rsp, 40
call _VN5ArrayIP6UsableE4initEx_rS1_
add rsp, 40
ret

.global _V3setP5ArrayIP6UsableES0_x
_V3setP5ArrayIP6UsableES0_x:
sub rsp, 40
xchg r8, rdx
call _VN5ArrayIP6UsableE3setExS0_
add rsp, 40
ret

.global _V6is_pigP7Vehicle_rb
_V6is_pigP7Vehicle_rb:
push rbx
push rsi
push rdi
sub rsp, 32
mov rsi, rcx
xor rdi, rdi
mov rcx, [rsi]
mov rbx, [rcx]
lea rcx, [rip+_VN3Pig_descriptor]
cmp rbx, rcx
jz _V6is_pigP7Vehicle_rb_L1
mov rcx, rbx
lea rdx, [rip+_VN3Pig_descriptor]
call _V8inheritsPhS__rx
test rax, rax
je _V6is_pigP7Vehicle_rb_L0
_V6is_pigP7Vehicle_rb_L1:
mov rdi, 1
_V6is_pigP7Vehicle_rb_L0:
mov rax, rdi
add rsp, 32
pop rdi
pop rsi
pop rbx
ret

_V4initv_rx:
push rbx
push rsi
push rdi
push rbp
push r12
sub rsp, 32
call _V11create_johnv_rP6Person
mov rbx, rax
call _V10create_pigv_rP3Pig
mov rsi, rax
call _V10create_busv_rP3Bus
mov rdi, rax
call _V10create_carv_rP3Car
mov rbp, rax
call _V13create_bananav_rP6Banana
mov rcx, 4
mov r12, rax
call _V12create_arrayx_rP5ArrayIP6UsableE
mov rcx, rax
xor rdx, rdx
lea r8, [rsi+8]
mov rsi, rax
call _VN5ArrayIP6UsableE3setExS0_
mov rcx, rsi
mov rdx, 1
lea r8, [rdi+8]
call _VN5ArrayIP6UsableE3setExS0_
mov rcx, rsi
mov rdx, 2
lea r8, [rbp+8]
call _VN5ArrayIP6UsableE3setExS0_
mov rcx, rsi
mov rdx, 3
mov r8, r12
call _VN5ArrayIP6UsableE3setExS0_
mov rcx, rsi
mov rdx, 10
call _V21get_reliable_vehiclesP5ArrayIP6UsableEx_rP4ListIP7VehicleE
mov rcx, rbx
mov rdx, rax
mov r8, 7000
call _V14choose_vehicleP6EntityP4ListIP7VehicleEx_rS2_
mov rax, 1
add rsp, 32
pop r12
pop rbp
pop rdi
pop rsi
pop rbx
ret

_V8allocatex_rPh:
push rbx
push rsi
sub rsp, 40
mov r8, [rip+_VN10Allocation_current]
test r8, r8
jz _V8allocatex_rPh_L0
mov rdx, [r8+16]
lea r9, [rdx+rcx]
cmp r9, 1000000
jg _V8allocatex_rPh_L0
lea r9, [rdx+rcx]
mov qword ptr [r8+16], r9
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
mov qword ptr [rax+8], rsi
mov qword ptr [rax+16], rbx
mov qword ptr [rip+_VN10Allocation_current], rax
mov rax, rsi
add rsp, 40
pop rsi
pop rbx
ret

_V8inheritsPhS__rx:
push rbx
push rsi
mov r8, [rcx]
mov r9, [rdx]
movzx r10, byte ptr [r9]
xor rax, rax
_V8inheritsPhS__rx_L1:
_V8inheritsPhS__rx_L0:
movzx rcx, byte ptr [r8+rax]
add rax, 1
cmp rcx, r10
jnz _V8inheritsPhS__rx_L4
mov r11, rcx
mov rbx, 1
_V8inheritsPhS__rx_L7:
_V8inheritsPhS__rx_L6:
movzx r11, byte ptr [r8+rax]
movzx rsi, byte ptr [r9+rbx]
add rax, 1
add rbx, 1
cmp r11, rsi
jz _V8inheritsPhS__rx_L9
cmp r11, 1
jne _V8inheritsPhS__rx_L9
test rsi, rsi
jne _V8inheritsPhS__rx_L9
mov rax, 1
pop rsi
pop rbx
ret
_V8inheritsPhS__rx_L9:
jmp _V8inheritsPhS__rx_L6
_V8inheritsPhS__rx_L8:
jmp _V8inheritsPhS__rx_L3
_V8inheritsPhS__rx_L4:
cmp rcx, 2
jne _V8inheritsPhS__rx_L3
xor rax, rax
pop rsi
pop rbx
ret
_V8inheritsPhS__rx_L3:
jmp _V8inheritsPhS__rx_L0
_V8inheritsPhS__rx_L2:
pop rsi
pop rbx
ret

_V4movePhxS_x:
push rbx
push rsi
push rdi
push rbp
sub rsp, 40
mov rbx, rcx
mov rcx, r9
mov rsi, rdx
mov rdi, r8
mov rbp, r9
call _V8allocatex_rPh
add rbx, rsi
mov rcx, rbx
mov rdx, rbp
mov r8, rax
mov rbx, rax
call _V4copyPhxS_
mov rcx, rbx
mov rdx, rbp
mov r8, rdi
call _V4copyPhxS_
add rsp, 40
pop rbp
pop rdi
pop rsi
pop rbx
ret

_V9length_ofPh_rx:
xor rax, rax
_V9length_ofPh_rx_L1:
_V9length_ofPh_rx_L0:
movzx rdx, byte ptr [rcx+rax]
test rdx, rdx
jne _V9length_ofPh_rx_L3
ret
_V9length_ofPh_rx_L3:
add rax, 1
jmp _V9length_ofPh_rx_L0
_V9length_ofPh_rx_L2:
ret

_V7requirebPh:
push rbx
push rsi
sub rsp, 40
mov rbx, rcx
mov rsi, rdx
test rbx, rbx
jne _V7requirebPh_L0
mov rcx, rsi
call _V7printlnPh
mov rcx, 1
call _V4exitx
_V7requirebPh_L0:
add rsp, 40
pop rsi
pop rbx
ret

_V5printP6String:
push rbx
sub rsp, 32
mov rbx, rcx
call _VN6String4dataEv_rPh
mov rcx, rbx
mov rbx, rax
call _VN6String6lengthEv_rx
mov rcx, rbx
mov rdx, rax
call _V14internal_printPhx
add rsp, 32
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

_VN6Person4initEPhxbb_rPS_:
push rbx
push rsi
push rdi
push rbp
sub rsp, 40
mov rbx, rcx
mov rcx, 34
mov rsi, rdx
mov rdi, r8
mov rbp, r9
call _V8allocatex_rPh
lea rcx, [rip+_VN6Person_configuration+8]
mov qword ptr [rax], rcx
mov rcx, rbx
mov rbx, rax
call _VN6String4initEPh_rPS_
mov qword ptr [rbx+16], rax
mov qword ptr [rbx+24], rsi
mov byte ptr [rbx+32], dil
mov byte ptr [rbx+33], bpl
mov rax, rbx
add rsp, 40
pop rbp
pop rdi
pop rsi
pop rbx
ret

_VN7Vehicle4timeEd_rd:
sub rsp, 40
movsd xmm1, qword ptr [rip+_VN7Vehicle4timeEd_rd_C0]
mulsd xmm1, xmm0
movsd xmm0, qword ptr [rcx+32]
divsd xmm1, xmm0
movsd xmm0, xmm1
call _V4sqrtd_rd
add rsp, 40
ret

_VN3Pig4initEv_rPS_:
sub rsp, 40
mov rcx, 58
call _V8allocatex_rPh
lea rcx, [rip+_VN3Pig_configuration+48]
mov qword ptr [rax+8], rcx
lea rcx, [rip+_VN3Pig_configuration+16]
mov qword ptr [rax+16], rcx
lea rcx, [rip+_VN3Pig_configuration+8]
mov qword ptr [rax], rcx
mov rcx, 4619567317775286272
mov qword ptr [rax+24], rcx
mov qword ptr [rax+32], 100
mov rcx, 4613937818241073152
mov qword ptr [rax+40], rcx
mov word ptr [rax+48], 1
add rsp, 40
ret

_VN3Pig5skillEv_rx_v:
sub rcx, 8
_VN3Pig5skillEv_rx:
mov rax, 1
ret

_VN3Pig11reliabilityEv_rx_v:
sub rcx, 8
_VN3Pig11reliabilityEv_rx:
mov rax, -1
ret

_VN3Pig5likesEP6Entity_rb_v:
sub rcx, 8
_VN3Pig5likesEP6Entity_rb:
push rbx
push rdi
push rsi
push rbp
push r12
sub rsp, 32
xor rsi, rsi
mov rbp, rdx
xor r12, r12
mov rdi, rbp
mov rcx, [rbp]
mov rbx, [rcx]
lea rcx, [rip+_VN6Person_descriptor]
cmp rbx, rcx
jz _VN3Pig5likesEP6Entity_rb_L4
mov rcx, rbx
lea rdx, [rip+_VN6Person_descriptor]
call _V8inheritsPhS__rx
test rax, rax
je _VN3Pig5likesEP6Entity_rb_L3
_VN3Pig5likesEP6Entity_rb_L4:
mov rsi, rdi
_VN3Pig5likesEP6Entity_rb_L3:
test rsi, rsi
jz _VN3Pig5likesEP6Entity_rb_L0
movzx rcx, byte ptr [rsi+33]
test rcx, rcx
jz _VN3Pig5likesEP6Entity_rb_L0
mov r12, 1
_VN3Pig5likesEP6Entity_rb_L0:
mov rax, r12
add rsp, 32
pop r12
pop rbp
pop rsi
pop rdi
pop rbx
ret

_VN3Car4initEv_rPS_:
sub rsp, 40
mov rcx, 58
call _V8allocatex_rPh
lea rcx, [rip+_VN3Car_configuration+48]
mov qword ptr [rax+8], rcx
lea rcx, [rip+_VN3Car_configuration+16]
mov qword ptr [rax+16], rcx
lea rcx, [rip+_VN3Car_configuration+8]
mov qword ptr [rax], rcx
mov rcx, 4632937379169042432
mov qword ptr [rax+24], rcx
mov qword ptr [rax+32], 1500
movsd xmm0, qword ptr [rip+_VN3Car4initEv_rPS__C0]
movsd qword ptr [rax+40], xmm0
mov word ptr [rax+48], 5
add rsp, 40
ret

_VN3Car5skillEv_rx_v:
sub rcx, 8
_VN3Car5skillEv_rx:
mov rax, 10
ret

_VN3Car11reliabilityEv_rx_v:
sub rcx, 8
_VN3Car11reliabilityEv_rx:
mov rax, 100
ret

_VN3Car5likesEP6Entity_rb_v:
sub rcx, 8
_VN3Car5likesEP6Entity_rb:
push rbx
push rdi
push rsi
push rbp
push r12
sub rsp, 32
xor rsi, rsi
mov rbp, rdx
xor r12, r12
mov rdi, rbp
mov rcx, [rbp]
mov rbx, [rcx]
lea rcx, [rip+_VN6Person_descriptor]
cmp rbx, rcx
jz _VN3Car5likesEP6Entity_rb_L4
mov rcx, rbx
lea rdx, [rip+_VN6Person_descriptor]
call _V8inheritsPhS__rx
test rax, rax
je _VN3Car5likesEP6Entity_rb_L3
_VN3Car5likesEP6Entity_rb_L4:
mov rsi, rdi
_VN3Car5likesEP6Entity_rb_L3:
test rsi, rsi
jz _VN3Car5likesEP6Entity_rb_L0
movzx rcx, byte ptr [rsi+32]
test rcx, rcx
jz _VN3Car5likesEP6Entity_rb_L0
mov r12, 1
_VN3Car5likesEP6Entity_rb_L0:
mov rax, r12
add rsp, 32
pop r12
pop rbp
pop rsi
pop rdi
pop rbx
ret

_VN6Banana4initEv_rPS_:
sub rsp, 40
mov rcx, 24
call _V8allocatex_rPh
lea rcx, [rip+_VN6Banana_configuration+24]
mov qword ptr [rax+8], rcx
lea rcx, [rip+_VN6Banana_configuration+8]
mov qword ptr [rax], rcx
add rsp, 40
ret

_VN6Banana5likesEP6Entity_rb_v:
_VN6Banana5likesEP6Entity_rb:
mov rax, 1
ret

_VN3Bus4initEv_rPS_:
sub rsp, 40
mov rcx, 58
call _V8allocatex_rPh
lea rcx, [rip+_VN3Bus_configuration+48]
mov qword ptr [rax+8], rcx
lea rcx, [rip+_VN3Bus_configuration+16]
mov qword ptr [rax+16], rcx
lea rcx, [rip+_VN3Bus_configuration+8]
mov qword ptr [rax], rcx
mov rcx, 4630826316843712512
mov qword ptr [rax+24], rcx
mov qword ptr [rax+32], 4000
movsd xmm0, qword ptr [rip+_VN3Bus4initEv_rPS__C0]
movsd qword ptr [rax+40], xmm0
mov word ptr [rax+48], 40
add rsp, 40
ret

_VN3Bus5skillEv_rx_v:
sub rcx, 8
_VN3Bus5skillEv_rx:
mov rax, 40
ret

_VN3Bus11reliabilityEv_rx_v:
sub rcx, 8
_VN3Bus11reliabilityEv_rx:
mov rax, 100
ret

_VN3Bus5likesEP6Entity_rb_v:
sub rcx, 8
_VN3Bus5likesEP6Entity_rb:
push rbx
push rdi
push rsi
push rbp
push r12
sub rsp, 32
xor rsi, rsi
mov rbp, rdx
xor r12, r12
mov rdi, rbp
mov rcx, [rbp]
mov rbx, [rcx]
lea rcx, [rip+_VN6Person_descriptor]
cmp rbx, rcx
jz _VN3Bus5likesEP6Entity_rb_L4
mov rcx, rbx
lea rdx, [rip+_VN6Person_descriptor]
call _V8inheritsPhS__rx
test rax, rax
je _VN3Bus5likesEP6Entity_rb_L3
_VN3Bus5likesEP6Entity_rb_L4:
mov rsi, rdi
_VN3Bus5likesEP6Entity_rb_L3:
test rsi, rsi
jz _VN3Bus5likesEP6Entity_rb_L0
movzx rcx, byte ptr [rsi+32]
test rcx, rcx
jz _VN3Bus5likesEP6Entity_rb_L0
mov r12, 1
_VN3Bus5likesEP6Entity_rb_L0:
mov rax, r12
add rsp, 32
pop r12
pop rbp
pop rsi
pop rdi
pop rbx
ret

_VN6String4initEPh_rPS_:
push rbx
sub rsp, 32
mov rbx, rcx
mov rcx, 16
call _V8allocatex_rPh
mov qword ptr [rax+8], rbx
add rsp, 32
pop rbx
ret

_VN6String6appendEh_rPS_:
push rbx
push rsi
push rdi
sub rsp, 32
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
call _V4copyPhxS_
mov byte ptr [rbx+rdi], sil
add rdi, 1
mov byte ptr [rbx+rdi], 0
mov rcx, rbx
call _VN6String4initEPh_rPS_
add rsp, 32
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
movzx r8, byte ptr [rdx+rax]
test r8, r8
je _VN6String6lengthEv_rx_L1
_VN6String6lengthEv_rx_L0:
add rax, 1
mov rdx, [rcx+8]
movzx r8, byte ptr [rdx+rax]
test r8, r8
jne _VN6String6lengthEv_rx_L0
_VN6String6lengthEv_rx_L1:
ret

_VN6String6equalsEPh_rb:
push rbx
push rsi
push rdi
sub rsp, 32
mov rbx, rcx
mov rsi, rdx
call _VN6String6lengthEv_rx
mov rcx, rsi
mov rdi, rax
call _V9length_ofPh_rx
cmp rdi, rax
je _VN6String6equalsEPh_rb_L0
xor rax, rax
add rsp, 32
pop rdi
pop rsi
pop rbx
ret
_VN6String6equalsEPh_rb_L0:
xor rcx, rcx
cmp rcx, rdi
jge _VN6String6equalsEPh_rb_L3
_VN6String6equalsEPh_rb_L2:
mov rdx, [rbx+8]
movzx r8, byte ptr [rdx+rcx]
movzx r9, byte ptr [rsi+rcx]
cmp r8, r9
jz _VN6String6equalsEPh_rb_L5
xor rax, rax
add rsp, 32
pop rdi
pop rsi
pop rbx
ret
_VN6String6equalsEPh_rb_L5:
add rcx, 1
cmp rcx, rdi
jl _VN6String6equalsEPh_rb_L2
_VN6String6equalsEPh_rb_L3:
mov rax, 1
add rsp, 32
pop rdi
pop rsi
pop rbx
ret

_VN5ArrayIP6UsableE4initEx_rS1_:
push rbx
push rsi
sub rsp, 40
mov rbx, rcx
mov rcx, 24
call _V8allocatex_rPh
xor rcx, rcx
test rbx, rbx
jl _VN5ArrayIP6UsableE4initEx_rS1__L0
mov rcx, 1
_VN5ArrayIP6UsableE4initEx_rS1__L0:
lea rdx, [rip+_VN5ArrayIP6UsableE4initEx_rS1__S0]
mov rsi, rax
call _V7requirebPh
mov rcx, rbx
sal rcx, 3
call _V8allocatex_rPh
mov qword ptr [rsi+8], rax
mov qword ptr [rsi+16], rbx
mov rax, rsi
add rsp, 40
pop rsi
pop rbx
ret

_VN5ArrayIP6UsableE3setExS0_:
push rbx
push rsi
push rdi
sub rsp, 32
xor rax, rax
test rdx, rdx
jl _VN5ArrayIP6UsableE3setExS0__L0
mov r9, [rcx+16]
cmp rdx, r9
jge _VN5ArrayIP6UsableE3setExS0__L0
mov rax, 1
_VN5ArrayIP6UsableE3setExS0__L0:
mov rbx, rcx
mov rcx, rax
mov rsi, rdx
lea rdx, [rip+_VN5ArrayIP6UsableE3setExS0__S0]
mov rdi, r8
call _V7requirebPh
mov rcx, [rbx+8]
sal rsi, 3
mov qword ptr [rcx+rsi], rdi
add rsp, 32
pop rdi
pop rsi
pop rbx
ret

_VN5ArrayIP6UsableE3getEx_rS0_:
push rbx
push rsi
sub rsp, 40
xor rax, rax
test rdx, rdx
jl _VN5ArrayIP6UsableE3getEx_rS0__L0
mov r8, [rcx+16]
cmp rdx, r8
jge _VN5ArrayIP6UsableE3getEx_rS0__L0
mov rax, 1
_VN5ArrayIP6UsableE3getEx_rS0__L0:
mov rbx, rcx
mov rcx, rax
mov rsi, rdx
lea rdx, [rip+_VN5ArrayIP6UsableE3getEx_rS0__S0]
call _V7requirebPh
mov rcx, [rbx+8]
sal rsi, 3
mov rax, [rcx+rsi]
add rsp, 40
pop rsi
pop rbx
ret

_VN4ListIP7VehicleE4initEv_rS1_:
push rbx
sub rsp, 32
mov rcx, 32
call _V8allocatex_rPh
mov rcx, 8
mov rbx, rax
call _V8allocatex_rPh
mov qword ptr [rbx+8], rax
mov qword ptr [rbx+16], 1
mov qword ptr [rbx+24], 0
mov rax, rbx
add rsp, 32
pop rbx
ret

_VN4ListIP7VehicleE4growEv:
push rbx
push rsi
sub rsp, 40
mov rdx, [rcx+16]
sal rdx, 1
sal rdx, 3
mov rbx, rcx
mov rcx, rdx
call _V8allocatex_rPh
mov rcx, [rbx+8]
mov rdx, [rbx+16]
sal rdx, 3
mov r8, rax
mov rsi, rax
call _V4copyPhxS_
mov qword ptr [rbx+8], rsi
mov rcx, [rbx+16]
sal rcx, 1
mov qword ptr [rbx+16], rcx
add rsp, 40
pop rsi
pop rbx
ret

_VN4ListIP7VehicleE3addES0_:
push rbx
push rsi
push rdi
sub rsp, 32
mov rsi, rcx
mov rbx, [rsi+24]
mov rcx, [rsi+16]
cmp rbx, rcx
jne _VN4ListIP7VehicleE3addES0__L0
mov rcx, rsi
mov rdi, rdx
call _VN4ListIP7VehicleE4growEv
mov rdx, rdi
_VN4ListIP7VehicleE3addES0__L0:
mov rcx, [rsi+8]
mov rbx, [rsi+24]
mov r8, rbx
sal r8, 3
mov qword ptr [rcx+r8], rdx
lea rcx, [rbx+1]
mov qword ptr [rsi+24], rcx
add rbx, 1
add rsp, 32
pop rdi
pop rsi
pop rbx
ret

_VN4ListIP7VehicleE6removeEx:
push rbp
push rbx
push rsi
push rdi
sub rsp, 40
lea rbp, [rdx+1]
sal rbp, 3
mov rbx, [rcx+24]
sub rbx, rdx
sub rbx, 1
sal rbx, 3
mov rsi, rcx
mov rdi, rdx
test rbx, rbx
jle _VN4ListIP7VehicleE6removeEx_L0
mov r8, [rsi+8]
sal rdi, 3
add r8, rdi
mov rcx, [rsi+8]
mov rdx, rbp
mov r9, rbx
call _V4movePhxS_x
_VN4ListIP7VehicleE6removeEx_L0:
sub qword ptr [rsi+24], 1
add rsp, 40
pop rdi
pop rsi
pop rbx
pop rbp
ret

_VN4ListIP7VehicleE3getEx_rS0_:
mov r8, [rcx+8]
sal rdx, 3
mov rax, [r8+rdx]
ret

_VN4ListIP7VehicleE4sizeEv_rx:
mov rax, [rcx+24]
ret

.section .data

_VN10Allocation_current:
.quad 0

_VN6Random_a:
.quad 0
_VN6Random_b:
.quad 0
_VN6Random_c:
.quad 0
_VN6Random_n:
.quad 0

_VN6Entity_configuration:
.quad _VN6Entity_descriptor

_VN6Entity_descriptor:
.quad _VN6Entity_descriptor_0
.long 8
.long 0

_VN6Entity_descriptor_0:
.ascii "Entity"
.byte 0
.byte 1
.byte 2
.byte 0

_VN6Person_configuration:
.quad _VN6Person_descriptor
.quad _VN6Person_descriptor

_VN6Person_descriptor:
.quad _VN6Person_descriptor_0
.long 34
.long 1
.quad _VN6Entity_descriptor

_VN6Person_descriptor_0:
.ascii "Person"
.byte 0
.byte 1
.ascii "Entity"
.byte 1
.byte 2
.byte 0

_VN6Usable_configuration:
.quad _VN6Usable_descriptor

_VN6Usable_descriptor:
.quad _VN6Usable_descriptor_0
.long 8
.long 0

_VN6Usable_descriptor_0:
.ascii "Usable"
.byte 0
.byte 1
.byte 2
.byte 0

_VN7Vehicle_configuration:
.quad _VN7Vehicle_descriptor
.quad _VN7Vehicle_descriptor

_VN7Vehicle_descriptor:
.quad _VN7Vehicle_descriptor_0
.long 42
.long 1
.quad _VN6Usable_descriptor

_VN7Vehicle_descriptor_0:
.ascii "Vehicle"
.byte 0
.byte 1
.ascii "Usable"
.byte 1
.byte 2
.byte 0

_VN8Drivable_configuration:
.quad _VN8Drivable_descriptor

_VN8Drivable_descriptor:
.quad _VN8Drivable_descriptor_0
.long 8
.long 0

_VN8Drivable_descriptor_0:
.ascii "Drivable"
.byte 0
.byte 1
.byte 2
.byte 0

_VN7Ridable_configuration:
.quad _VN7Ridable_descriptor

_VN7Ridable_descriptor:
.quad _VN7Ridable_descriptor_0
.long 8
.long 0

_VN7Ridable_descriptor_0:
.ascii "Ridable"
.byte 0
.byte 1
.byte 2
.byte 0

_VN3Pig_configuration:
.quad _VN3Pig_descriptor
.quad _VN3Pig_descriptor
.quad _VN3Pig_descriptor
.quad _VN3Pig5skillEv_rx_v
.quad _VN3Pig11reliabilityEv_rx_v
.quad _VN3Pig5likesEP6Entity_rb_v
.quad _VN3Pig_descriptor
.quad _VN3Pig5likesEP6Entity_rb_v

_VN3Pig_descriptor:
.quad _VN3Pig_descriptor_0
.long 58
.long 2
.quad _VN7Ridable_descriptor
.quad _VN7Vehicle_descriptor

_VN3Pig_descriptor_0:
.ascii "Pig"
.byte 0
.byte 1
.ascii "Ridable"
.byte 1
.ascii "Vehicle"
.byte 1
.ascii "Usable"
.byte 1
.byte 2
.byte 0

_VN3Car_configuration:
.quad _VN3Car_descriptor
.quad _VN3Car_descriptor
.quad _VN3Car_descriptor
.quad _VN3Car5skillEv_rx_v
.quad _VN3Car11reliabilityEv_rx_v
.quad _VN3Car5likesEP6Entity_rb_v
.quad _VN3Car_descriptor
.quad _VN3Car5likesEP6Entity_rb_v

_VN3Car_descriptor:
.quad _VN3Car_descriptor_0
.long 58
.long 2
.quad _VN8Drivable_descriptor
.quad _VN7Vehicle_descriptor

_VN3Car_descriptor_0:
.ascii "Car"
.byte 0
.byte 1
.ascii "Drivable"
.byte 1
.ascii "Vehicle"
.byte 1
.ascii "Usable"
.byte 1
.byte 2
.byte 0

_VN6Banana_configuration:
.quad _VN6Banana_descriptor
.quad _VN6Banana_descriptor
.quad _VN6Banana5likesEP6Entity_rb_v
.quad _VN6Banana_descriptor

_VN6Banana_descriptor:
.quad _VN6Banana_descriptor_0
.long 24
.long 2
.quad _VN6Usable_descriptor
.quad _VN6Entity_descriptor

_VN6Banana_descriptor_0:
.ascii "Banana"
.byte 0
.byte 1
.ascii "Usable"
.byte 1
.ascii "Entity"
.byte 1
.byte 2
.byte 0

_VN3Bus_configuration:
.quad _VN3Bus_descriptor
.quad _VN3Bus_descriptor
.quad _VN3Bus_descriptor
.quad _VN3Bus5skillEv_rx_v
.quad _VN3Bus11reliabilityEv_rx_v
.quad _VN3Bus5likesEP6Entity_rb_v
.quad _VN3Bus_descriptor
.quad _VN3Bus5likesEP6Entity_rb_v

_VN3Bus_descriptor:
.quad _VN3Bus_descriptor_0
.long 58
.long 2
.quad _VN8Drivable_descriptor
.quad _VN7Vehicle_descriptor

_VN3Bus_descriptor_0:
.ascii "Bus"
.byte 0
.byte 1
.ascii "Drivable"
.byte 1
.ascii "Vehicle"
.byte 1
.ascii "Usable"
.byte 1
.byte 2
.byte 0

_VN4Page_configuration:
.quad _VN4Page_descriptor

_VN4Page_descriptor:
.quad _VN4Page_descriptor_0
.long 24
.long 0

_VN4Page_descriptor_0:
.ascii "Page"
.byte 0
.byte 1
.byte 2
.byte 0

_VN10Allocation_configuration:
.quad _VN10Allocation_descriptor

_VN10Allocation_descriptor:
.quad _VN10Allocation_descriptor_0
.long 8
.long 0

_VN10Allocation_descriptor_0:
.ascii "Allocation"
.byte 0
.byte 1
.byte 2
.byte 0

_VN6String_configuration:
.quad _VN6String_descriptor

_VN6String_descriptor:
.quad _VN6String_descriptor_0
.long 16
.long 0

_VN6String_descriptor_0:
.ascii "String"
.byte 0
.byte 1
.byte 2
.byte 0

_VN5Array_configuration:
.quad _VN5Array_descriptor

_VN5Array_descriptor:
.quad _VN5Array_descriptor_0
.long 8
.long 0

_VN5Array_descriptor_0:
.ascii "Array"
.byte 0
.byte 1
.byte 2
.byte 0

_VN5Sheet_configuration:
.quad _VN5Sheet_descriptor

_VN5Sheet_descriptor:
.quad _VN5Sheet_descriptor_0
.long 8
.long 0

_VN5Sheet_descriptor_0:
.ascii "Sheet"
.byte 0
.byte 1
.byte 2
.byte 0

_VN3Box_configuration:
.quad _VN3Box_descriptor

_VN3Box_descriptor:
.quad _VN3Box_descriptor_0
.long 8
.long 0

_VN3Box_descriptor_0:
.ascii "Box"
.byte 0
.byte 1
.byte 2
.byte 0

_VN4List_configuration:
.quad _VN4List_descriptor

_VN4List_descriptor:
.quad _VN4List_descriptor_0
.long 8
.long 0

_VN4List_descriptor_0:
.ascii "List"
.byte 0
.byte 1
.byte 2
.byte 0

_VN6Random_configuration:
.quad _VN6Random_descriptor

_VN6Random_descriptor:
.quad _VN6Random_descriptor_0
.long 8
.long 0

_VN6Random_descriptor_0:
.ascii "Random"
.byte 0
.byte 1
.byte 2
.byte 0

_VN5ArrayIP6UsableE_configuration:
.quad _VN5ArrayIP6UsableE_descriptor

_VN5ArrayIP6UsableE_descriptor:
.quad _VN5ArrayIP6UsableE_descriptor_0
.long 24
.long 0

_VN5ArrayIP6UsableE_descriptor_0:
.ascii "Array<Usable>"
.byte 0
.byte 1
.byte 2
.byte 0

_VN4ListIP7VehicleE_configuration:
.quad _VN4ListIP7VehicleE_descriptor

_VN4ListIP7VehicleE_descriptor:
.quad _VN4ListIP7VehicleE_descriptor_0
.long 32
.long 0

_VN4ListIP7VehicleE_descriptor_0:
.ascii "List<Vehicle>"
.byte 0
.byte 1
.byte 2
.byte 0

.balign 16
_V14choose_vehicleP6EntityP4ListIP7VehicleEd_rS2__S0:
.ascii "Steve"
.byte 0
.balign 16
_V11create_johnv_rP6Person_S0:
.ascii "John"
.byte 0
.balign 16
_V10create_maxv_rP6Person_S0:
.ascii "Max"
.byte 0
.balign 16
_V11create_gabev_rP6Person_S0:
.ascii "Gabe"
.byte 0
.balign 16
_V12create_stevev_rP6Person_S0:
.ascii "Steve"
.byte 0
.balign 16
_VN5ArrayIP6UsableE3setExS0__S0:
.ascii "Index out of bounds"
.byte 0
.balign 16
_VN5ArrayIP6UsableE3getEx_rS0__S0:
.ascii "Index out of bounds"
.byte 0
.balign 16
_VN5ArrayIP6UsableE4initEx_rS1__S0:
.ascii "Tried to create a standard array but its size was a negative value"
.byte 0
.balign 16
_VN7Vehicle4timeEd_rd_C0:
.byte 0, 0, 0, 0, 0, 0, 0, 64 # 2.0
.balign 16
_VN3Car4initEv_rPS__C0:
.byte 184, 30, 133, 235, 81, 56, 22, 64 # 5.555
.balign 16
_VN3Bus4initEv_rPS__C0:
.byte 0, 0, 0, 0, 0, 0, 4, 64 # 2.5