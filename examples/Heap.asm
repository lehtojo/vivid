section .text

PERMISSION_READ equ 0x1
PERMISSION_WRITE equ 0x2
PERMISSION_EXECUTE equ 0x4

VISIBILITY_PRIVATE equ 0x02
VISIBILITY_PUBLIC equ 0x01

; EBP+16:   Visibility
; EBP+12:   Permissions
; EBP+8:    Bytes
; EBP+4:    Return address
; EBP+0:    Saved EBP
; EBP
system_allocate:
push ebp
mov ebp, esp
    
; EBP
; EBP-4:    off_t offset,
; EBP-8:    int fd, 
; EBP-12:   int flags,    
; EBP-16:   int prot, 
; EBP-20:   size_t length, 
; EBP-24:   void *addr
; ESP

push dword 0    ; Offset
push dword -1   ; FD

; Flags
mov eax, 0x20
mov ebx, [ebp+16]
or eax, ebx
push eax

push dword [ebp+12]   ; Protocol
push dword [ebp+8]    ; Length
push dword 0    ; Address

; MMAP
mov eax, 0x5a
mov ebx, esp
int 0x80

; Clean up
mov esp, ebp
pop ebp
ret

global _start
_start:

push dword VISIBILITY_PRIVATE ; Visibility

mov eax, PERMISSION_READ
mov ebx, PERMISSION_WRITE
or eax, ebx

lea ecx, [eax*ebx+0]

push eax ; Permissions
push dword 1000 ; Bytes

call system_allocate

mov edi, eax
mov [eax+374], dword 253

mov esi, eax
mov eax, 0
mov eax, dword [esi+374]

; Exit
mov eax, 1
mov ebx, 0
int 80h