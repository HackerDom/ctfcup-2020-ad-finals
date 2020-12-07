from django.urls import path

from . import views

app_name = 'main'
urlpatterns = [
    path('', views.index, name='index'),
    path('login/', views.login, name='login'),
    path('logout/', views.logout, name='logout'),
    path('signup/', views.signup, name="signup"),
    path('add_bark/', views.add_bark, name="add_bark"),
    path('get_bark/<int:bark_id>/', views.get_bark, name="get_bark"),
    path('leave_comment/<int:bark_id>/', views.leave_comment, name="leave_comment"),
    path('friends/', views.show_friends, name="show_friends"),
    path('profile/', views.get_profile, name="get_profile"),
    path('generate_token/', views.generate_token, name="generate_token"),
    path('confirm_friend/<str:username>', views.confirm_friend, name="confirm_friend"),
    path('revoke_friend/<str:username>', views.revoke_friend, name="revoke_friend"),
    path('add_friend/<str:username>', views.add_friend, name="add_friend"),
    path('<str:username>/', views.get_barks, name="get_barks"),
]
