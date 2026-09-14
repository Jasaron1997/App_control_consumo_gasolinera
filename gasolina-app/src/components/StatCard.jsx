export default function StatCard({ titulo, valor, unidad }) {
  return (
    <div className="bg-superficie border border-borde rounded-[10px] p-4 flex flex-col gap-1.5">
      <span className="text-[0.85rem] text-texto-sutil">{titulo}</span>
      <span className="text-[1.6rem] font-semibold">
        {valor}
        {unidad && <span className="text-[0.9rem] text-texto-sutil font-normal"> {unidad}</span>}
      </span>
    </div>
  )
}
