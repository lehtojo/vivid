[section .text]

global function_allocate:function
function_allocate:

;   System call parameters
;
;   off_t offset,
;   int fd, 
;   int flags,    
;   int prot, 
;   size_t length, 
;   void *addr
;
push dword 0
push dword -1
push dword 0x22 ; HARDWARE_MEMORY | VISIBILITY_PRIVATE
push dword 0x03 ; PERMISSION_READ | PERMISSION_WRITE
push dword [esp+16] ; Parameter: Region size
push dword 0

; push qword 0xFFFFFFFF ; 0 | -1
; push qword 0x0000002200000003 ; HARDWARE_MEMORY | VISIBILITY_PRIVATE + PERMISSION_READ | PERMISSION_WRITE
; push dword [esp+16] ; Parameter: Region size
; push dword 0

; System call: mmap
mov eax, 0x5a
mov ebx, esp
int 0x80

; Cleanup
add esp, 24
ret

global function_copy_0:function
function_copy_0:

; Parameters
; esp+12: destination
; esp+8: count
; esp+4: source
; esp+0: return address

mov edi, [esp+12]
mov ecx, [esp+8]
mov esi, [esp+4]

rep movsb

ret

global function_copy_1:function
function_copy_1:

; Parameters
; esp+16: offset
; esp+12: destination
; esp+8: count
; esp+4: source
; esp+0: return address

mov edi, [esp+12]
add edi, [esp+16]
mov ecx, [esp+8]
mov esi, [esp+4]

rep movsb

ret