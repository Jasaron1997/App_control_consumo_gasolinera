export default function StatCard({ titulo, valor, unidad }) {
  return (
    <div className="stat-card">
      <span className="stat-card__titulo">{titulo}</span>
      <span className="stat-card__valor">
        {valor}
        {unidad && <span className="stat-card__unidad"> {unidad}</span>}
      </span>
    </div>
  )
}
