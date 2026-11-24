export default function Pagination({ page, totalPages, onChange }: { page:number; totalPages:number; onChange:
(p:number)=>void }) {
 if (totalPages <= 1) return null
 return <div className="pagination">
 <button disabled={page<=1} onClick={()=>onChange(page-1)}>Previous</button>
 <span>Page {page} of {totalPages}</span>
 <button disabled={page>=totalPages} onClick={()=>onChange(page+1)}>Next</button>
 </div>
}