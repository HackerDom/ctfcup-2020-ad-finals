from django.urls import path

from . import views

app_name = 'api'
urlpatterns = [
    path('', views.index, name='index'),
    path('barks/<str:username>/', views.get_barks, name='get_barks'),
    path('comments/<int:bark_id>/', views.get_comments, name='get_comments'),
    path('user/<str:username>', views.get_user, name='get_user'),
    path('last_barks/<int:page_n>', views.get_last_barks, name='get_last_barks'),
    path('users/<int:page_n>', views.get_users, name='get_users')
]
