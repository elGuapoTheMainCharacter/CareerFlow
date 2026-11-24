import { useNavigate } from 'react-router'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { notificationsApi } from '../api/notifications'
import type { NotificationItem } from '../types'

export default function NotificationsPage() {
  const qc = useQueryClient()
  const navigate = useNavigate()
  const list = useQuery({ queryKey: ['notifications'], queryFn: notificationsApi.list })

  const refresh = () => {
    qc.invalidateQueries({ queryKey: ['notifications'] })
    qc.invalidateQueries({ queryKey: ['notifications-count'] })
  }

  const read = useMutation({ mutationFn: notificationsApi.markRead, onSuccess: refresh })
  const readAll = useMutation({ mutationFn: notificationsApi.markAllRead, onSuccess: refresh })

  function open(n: NotificationItem) {
    if (!n.isRead) read.mutate(n.id)
    if (n.link) navigate(n.link)
  }

  return (
    <>
      <div className="page-title">
        <div>
          <h1>Notifications</h1>
          <p>{list.data ? `${list.data.unreadCount} unread` : ' '}</p>
        </div>
        <button disabled={!list.data || list.data.unreadCount === 0 || readAll.isPending} onClick={() => readAll.mutate()}>
          Mark all as read
        </button>
      </div>

      {list.isPending && <p>Loading...</p>}
      {list.isError && <p className="error">{list.error.message}</p>}
      {list.data?.items.length === 0 && <p>Nothing yet. Updates about applications and invitations show up here.</p>}

      {list.data?.items.map(n => (
        <article
          key={n.id}
          className={`row-card notif${n.isRead ? '' : ' unread'}`}
          style={{ cursor: n.link ? 'pointer' : 'default' }}
          onClick={() => open(n)}
        >
          <div>
            <b>{n.message}</b>
            <small>{new Date(n.createdAtUtc).toLocaleString()}</small>
          </div>
        </article>
      ))}
    </>
  )
}