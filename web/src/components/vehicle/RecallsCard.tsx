import { clean, formatDate } from "../../lib/format";
import { he } from "../../strings.he";
import type { VehicleRecallRecord } from "../../types/vehicle";
import { Icon } from "../Icon";
import { Ltr, Reveal } from "../ui";

export function RecallsCard({ recalls }: { recalls: VehicleRecallRecord[] }) {
  return (
    <section id="recalls" className="card recalls" aria-labelledby="recalls-title">
      <header className="recalls__header">
        <span className="recalls__icon" aria-hidden="true">
          <Icon name="recall" size={26} />
        </span>
        <div>
          <h2 id="recalls-title" className="recalls__title">
            {he.recalls.title(recalls.length)}
          </h2>
          <p className="recalls__note">{he.recalls.note}</p>
        </div>
      </header>
      <ul className="recalls__list">
        {recalls.map((recall, i) => {
          const opened = formatDate(recall.openedDate);
          return (
            <Reveal as="li" key={recall.id ?? recall.recallId ?? i} index={i} className="recall">
              <div className="recall__top">
                {clean(recall.recallType) && <strong className="recall__type">{recall.recallType}</strong>}
                {clean(recall.recallId) && (
                  <span className="recall__id">
                    {he.recalls.recallId}: <Ltr>{recall.recallId}</Ltr>
                  </span>
                )}
              </div>
              {clean(recall.faultType) && (
                <p className="recall__fault">
                  <span className="recall__label">{he.recalls.faultType}:</span> {recall.faultType}
                </p>
              )}
              {clean(recall.faultDescription) && (
                <p className="recall__desc">
                  <span className="recall__label">{he.recalls.faultDescription}:</span> {recall.faultDescription}
                </p>
              )}
              {opened && (
                <p className="recall__date">
                  <Icon name="calendar" size={16} />
                  {he.recalls.openedDate}: <Ltr>{opened}</Ltr>
                </p>
              )}
            </Reveal>
          );
        })}
      </ul>
    </section>
  );
}
